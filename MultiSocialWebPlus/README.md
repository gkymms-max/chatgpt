# MultiSocialWebPlus (starter)

Bu repository örneği bir başlangıç WinForms uygulamasıdır:
- Sol menü: Müşteriler / Ürünler / Teklifler / Kategori / Sosyal Bağlantılar
- Veriler SQLite + EF Core ile LocalAppData altında saklanır
- Sosyal Bağlantılar: WebView2 tab'ları; her oturum için ayrı profil klasörü (çoklu WhatsApp oturumu gibi)

## Gereksinimler
- .NET SDK 8.0 (veya 6/7) yüklü
- WebView2 Runtime (Edge) kurulu (https://developer.microsoft.com/en-us/microsoft-edge/webview2/)

## Çalıştırma (CLI)

1. Klasöre git:
   ```
   cd MultiSocialWebPlus
   ```

2. Bağımlılıkları yükle:
   ```
   dotnet restore
   ```

3. Çalıştır:
   ```
   dotnet run
   ```

Veya Visual Studio'dan `.csproj` dosyasını açıp çalıştırabilirsiniz.

## NuGet Paketleri
- Microsoft.EntityFrameworkCore.Sqlite
- Microsoft.EntityFrameworkCore.Tools
- Microsoft.Web.WebView2

## Notlar
- Bu MVP örneği; arayüz ve iş akışları geliştirilebilir. Teklif oluşturucu basit (örnek) mantık içerir; detaylı satır ekleme/fiyatlama istersen geliştirilebilir.
- Sosyal bölümü WebView2 kullandığından `web.whatsapp.com` gibi servisler WebView2 içinde çalışacaktır; bazı servislerin embed/oturum politikaları değişebilir.
- Her sosyal tab ayrı bir profil klasörü kullanır, böylece aynı anda birden fazla hesapla oturum açılabilir.
