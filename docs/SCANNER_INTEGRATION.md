# Scanner Integration Guide

InvenScan keeps **all** scanner interaction behind a single interface,
`ScannerContract`. The app ships with a fully working `MockScanner` so you can
build, demo, and test without any hardware. To support a real device you write
**one** class that adapts your vendor SDK to `ScannerContract` and register it —
no feature/UI code changes.

> **Rule:** vendor SDK imports must live *only* inside your implementation
> class. Nothing else in the codebase may reference a hardware SDK.

---

## 1. The contract

`app/src/main/java/com/invenscan/app/scanner/ScannerContract.kt`

```kotlin
interface ScannerContract {
    fun initialize(context: Context, listener: ScanListener)
    fun startScan()
    fun stopScan()
    fun release()
    fun isReady(): Boolean

    interface ScanListener {
        fun onScanResult(code: String, type: ScanType)
        fun onScanError(message: String)
        fun onScannerDisconnected()
    }

    enum class ScanType { RFID, BARCODE }
}
```

| Method | When it is called | What you must do |
|--------|-------------------|------------------|
| `initialize(context, listener)` | When a scanning screen opens | Acquire the SDK reader, keep the `listener`. |
| `startScan()` | User presses Start (or trigger) | Begin a read; on each tag/barcode call `listener.onScanResult(code, type)`. |
| `stopScan()` | User presses Stop | Stop the read. |
| `release()` | Screen closes | Free the SDK reader, drop the `listener`. |
| `isReady()` | Before scanning | Return whether the reader is connected/usable. |

The `ScannerManager` singleton holds the active scanner and defaults to
`MockScanner`:

```kotlin
@Singleton
class ScannerManager @Inject constructor() {
    private var scanner: ScannerContract = MockScanner()
    fun setScanner(scanner: ScannerContract) { this.scanner = scanner }
    fun getScanner(): ScannerContract = scanner
    fun isReady(): Boolean = scanner.isReady()
}
```

---

## 2. Example — Zebra (EMDK / DataWedge-style RFID + barcode)

`app/src/main/java/com/invenscan/app/scanner/ZebraScanner.kt`

```kotlin
class ZebraScanner : ScannerContract {

    private var listener: ScannerContract.ScanListener? = null
    private var reader: ZebraReader? = null   // from the Zebra SDK

    override fun initialize(context: Context, listener: ScannerContract.ScanListener) {
        this.listener = listener
        reader = ZebraReader(context).apply {
            onBarcode = { code -> this@ZebraScanner.listener?.onScanResult(code, ScannerContract.ScanType.BARCODE) }
            onRfidTag = { epc  -> this@ZebraScanner.listener?.onScanResult(epc,  ScannerContract.ScanType.RFID) }
            onError   = { msg  -> this@ZebraScanner.listener?.onScanError(msg) }
            onDisconnected = { this@ZebraScanner.listener?.onScannerDisconnected() }
            connect()
        }
    }

    override fun startScan() { reader?.startInventory() }
    override fun stopScan()  { reader?.stopInventory() }
    override fun release()   { reader?.disconnect(); reader = null; listener = null }
    override fun isReady()   = reader?.isConnected == true
}
```

## 3. Example — Honeywell (AIDC / DataCollection)

`app/src/main/java/com/invenscan/app/scanner/HoneywellScanner.kt`

```kotlin
class HoneywellScanner : ScannerContract {

    private var listener: ScannerContract.ScanListener? = null
    private var barcodeReader: BarcodeReader? = null   // Honeywell AIDC SDK

    override fun initialize(context: Context, listener: ScannerContract.ScanListener) {
        this.listener = listener
        val manager = AidcManager.create(context)
        barcodeReader = manager.createBarcodeReader().apply {
            addBarcodeListener { event ->
                this@HoneywellScanner.listener?.onScanResult(
                    event.barcodeData, ScannerContract.ScanType.BARCODE
                )
            }
            setProperty("DEC_CODE39_ENABLED", true)
            claim()
        }
    }

    override fun startScan() { barcodeReader?.softwareTrigger(true) }
    override fun stopScan()  { barcodeReader?.softwareTrigger(false) }
    override fun release()   { barcodeReader?.close(); barcodeReader = null; listener = null }
    override fun isReady()   = barcodeReader != null
}
```

## 4. Example — Denso (CommScanner / SP1 RFID)

`app/src/main/java/com/invenscan/app/scanner/DensoScanner.kt`

```kotlin
class DensoScanner : ScannerContract {

    private var listener: ScannerContract.ScanListener? = null
    private var commScanner: CommScanner? = null   // Denso SDK

    override fun initialize(context: Context, listener: ScannerContract.ScanListener) {
        this.listener = listener
        commScanner = CommManager.getCommScanner(context).apply {
            setDataDelegate { data ->
                val type = if (isRfid(data)) ScannerContract.ScanType.RFID
                           else ScannerContract.ScanType.BARCODE
                this@DensoScanner.listener?.onScanResult(data.toString(), type)
            }
            setStatusDelegate { status ->
                if (status == DISCONNECTED) this@DensoScanner.listener?.onScannerDisconnected()
            }
            claim()
        }
    }

    override fun startScan() { commScanner?.startRead() }
    override fun stopScan()  { commScanner?.stopRead() }
    override fun release()   { commScanner?.close(); commScanner = null; listener = null }
    override fun isReady()   = commScanner != null
}
```

> The SDK type names above (`ZebraReader`, `AidcManager`, `CommScanner`, …) are
> illustrative — replace them with the exact classes from the vendor SDK you add
> to `app/build.gradle.kts`.

---

## 5. Register your scanner with Hilt

`ScannerManager` is already provided by Hilt (constructor `@Inject`, `@Singleton`).
Pick the implementation at startup, e.g. based on the value saved by the Settings
screen (`PrefManager.deviceId` / a scanner-type preference):

```kotlin
@HiltAndroidApp
class InvenScanApp : Application() {

    @Inject lateinit var scannerManager: ScannerManager
    @Inject lateinit var prefManager: PrefManager

    override fun onCreate() {
        super.onCreate()
        val scanner: ScannerContract = when (prefManager.scannerType) {
            "ZEBRA"     -> ZebraScanner()
            "HONEYWELL" -> HoneywellScanner()
            "DENSO"     -> DensoScanner()
            else        -> MockScanner()
        }
        scannerManager.setScanner(scanner)
    }
}
```

Prefer to bind it through DI instead? Provide it from a module:

```kotlin
@Module
@InstallIn(SingletonComponent::class)
object ScannerModule {
    @Provides @Singleton
    fun provideScanner(): ScannerContract = ZebraScanner()   // your device
}
```

…then have `ScannerManager` take `ScannerContract` as a constructor parameter.

---

## 6. Switch scanner at runtime

`ScannerManager.setScanner(...)` swaps the active implementation at any time:

```kotlin
scannerManager.getScanner().release()      // tear down current reader
scannerManager.setScanner(HoneywellScanner())
```

The Settings screen exposes a scanner-type selector (Mock / Zebra / Honeywell /
Denso) intended to drive exactly this — persist the choice in `PrefManager` and
apply it on next app start (or immediately, as above).

---

## 7. Testing without hardware

`MockScanner` implements the full contract and exposes test helpers:

```kotlin
val mock = scannerManager.getScanner() as MockScanner
mock.simulateScan("E2000017221101441890A1B1", ScannerContract.ScanType.RFID)
mock.simulateScan("ITM-001", ScannerContract.ScanType.BARCODE)
mock.simulateError("Reader timeout")
mock.simulateDisconnect()
```

`startScan()` must have been called first (the mock only emits while running),
mirroring real hardware behaviour.
