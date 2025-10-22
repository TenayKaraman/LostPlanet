Kayıp Gezegen (Lost Planet) – 2D Top-Down (Portrait 9×16)

Mobil odaklı, 2D top-down, grid tabanlı kayarak hareket (slide) mekaniğine sahip roguelite/aksiyon-strateji oyunu.
Proje; portrait (dikey) düzende 9×16 grid’i hedefler ve Mario Kart tarzı “sürpriz kutu → yetenek kartı” mantığını kullanır.

İçindekiler
- Oynanış Özeti
- Mimari • Klasör Yapısı
- Kurulum • Geliştirme
- Çalıştırma • Test Sahneleri
- UI Düzeni (TopBar & BottomBar)
- Önemli Scriptler
- Inputlar
- Build Ayarları (Mobil)
- Versiyonlama • PR Akışı
- Kodlama Standartları • Notlar

Oynanış Özeti
- Hareket: Oyuncu grid üzerinde kayarak ilerler; bir engele çarpınca durur.
- NOS (Süper Hız): 30 sn temiz kayış → NOS butonu aktif; bastığında düşman/engellerin içinden faz gibi geçer.
- Sürpriz Kutular: Kırınca yetenek kartı verir (Bomb / Shield / EMP / Phase / Freeze…). 3 slot’a kadar depolayıp istenen anda kullanılır.
- Kristaller: Tek renk; yeterli sayıda toplanınca portal aktif olur.
- Portal: Oyuncu girince dünya donar → kısa fade → Level Complete.

Mimari • Klasör Yapısı
Assets/
  Scripts/
    Core/            # GameManager, SaveManager, UIManager, AudioManager, FreezeService
    GridSystem/      # GridManager, GridEntity, CellData
    Gameplay/        # PlayerController, PlayerNOS, AbilityInventory, vb.
    Managers/        # CrystalManager, LifeManager
    Items/           # Crystal, PowerUpBox, PortalController  (Portal BURADA)
    UI/              # HUD / TopBar bağları, icon registry, UIFader
    Dev/             # AutoTestSceneBootstrap, runtime helperlar
Not: PortalController LostPlanet.Items namespace’indedir.

Kurulum • Geliştirme
1) Unity 2022.3+ (LTS) önerilir.
2) Packages:
   - Input System (Yeni Input)
   - DOTween (varsa – animasyon için; yoksa proje yine çalışır)
3) Editor Ayarları:
   - Edit → Project Settings → Editor
     - Version Control: Visible Meta Files
     - Asset Serialization: Force Text
4) Git LFS (önerilir): büyük dosyalar (ses/görsel/fbx) için.

Çalıştırma • Test Sahneleri
- Boş bir sahne açıp Dev/AutoTestSceneBootstrap script’ini bir GameObject’e ekleyin.
  Oyun; Grid (9×16), oyuncu, kristaller, kutular, tuzaklar, portal ve yöneticilerle runtime kurulur.

UI Düzeni (TopBar & BottomBar)
- TopBar: Kristal sayacı, Level adı, NOS bar (slider), can (kalp + sayı).
- BottomBar (AbilityBar): 3 slot; kart ikonları ve cooldown overlay’i.
Not: BottomBar Canvas’ta anchored yerleşmelidir. posY=0’da taşma varsa Canvas Scaler → Scale With Screen Size ve ReferenceResolution ayarlarını kontrol edin.

Önemli Scriptler
- Core/GameManager
  - Durum makinesi: Playing / Paused / LevelComplete / GameOver
  - OnLevelComplete(): FreezeService.FreezeWorld() çağırır, LevelComplete UI açar.
  - Pause/Resume: FreezeService kullanır.
  - LoadNextLevelWithFade() (opsiyonel): Fade’li seviye geçişi.
- Core/FreezeService
  - Merkezî donma: Time.timeScale=0 + Physics2D.simulationMode=Script + (varsa) DOTween.PauseAll()
  - Açma: ters işlemler.
- Managers/CrystalManager
  - OnCrystalCollected() → UI güncelle, eşik dolunca Items.PortalController.Activate().
- Items/PortalController
  - autoActivateByCrystals açıkken kristaller tamamlanınca otomatik aktif.
  - Oyuncu girince: Freeze → (varsA UIFader ile) kısa fade → GameManager.OnLevelComplete().
- Gameplay/PlayerNOS
  - Sadece GameState.Playing iken charge eder/günceller.
- Gameplay/AbilityInventory
  - Kart ekleme, slot kullanma, UI senkronizasyonu.
- GridSystem/GridManager
  - 9×16 portrait grid, world↔grid dönüşümleri.

Inputlar
- Swipe: Yön seçimi (Input System).
- NOS (Super): Buton (UI) veya mapped input.

Build Ayarları (Mobil)
- Orientation: Portrait.
- Safe Area: UI Canvas’ta safe area uyumu önerilir.
- Reference Resolution: 1080×1920 (veya 900×1600); Match 0.5–0.7.
- Android/iOS: IL2CPP + ARM64 önerilir.

Versiyonlama • PR Akışı
- Branch modeli:
  - main (stabil)
  - feature/*, fix/* dalları → PR ile main’e.
- PR içeriği:
  - Kısa başlık, ne/neden/nasıl test açıklaması, UI değiştiyse ekran görüntüsü/GIF
  - Küçük ve odaklı değişiklikler.
- Koruma: main’e doğrudan push yok, en az 1 review.

Kodlama Standartları • Notlar
- Namespace’ler klasör hiyerarşisini izler (LostPlanet.Core, LostPlanet.Items ...).
- Unity null propagation (?.) uyarılarından kaçınmak için klasik if (obj != null) tercih edilir.
- Update çalışan sistemlerde state guard kullanın:
  var gm = GameManager.Instance;
  if (gm != null && gm.State != GameState.Playing) return;
- Sahne bağımlılığını azaltmak için mümkün olduğunca runtime bootstrap ve FindObjectOfType ile “soft” referanslar; prod’da ScriptableObject-tabanlı servis yerleştirici (Service Locator) tercih edilebilir.

Hızlı Kontrol Listesi
- [ ] Input System aktif
- [ ] Canvas Scaler doğru
- [ ] Grid 9×16
- [ ] Crystal → Portal otomatik aktif
- [ ] Portal → Freeze + (fade) + LevelComplete
- [ ] NOS sadece Playing’de dolar
- [ ] PR’lar küçük ve tek konulu
