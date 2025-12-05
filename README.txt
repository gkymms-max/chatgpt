MultiSocial Web v8.1 — Responsive (kendini ölçeklendirme)
==============================================================
- **Sağ panel genişliği**: pencerenin ~%32’si, 360–560px arası otomatik ayarlanır.
- **Üst buton satırları**: `WrapContents = true` — dar ekranda otomatik alt satıra iner.
- **Sekmeler**: `Multiline` + dinamik sekme genişliği; küçülünce ikinci satıra geçer.
- **Sağ panel** `AutoScroll` — yükseklik kısa kalırsa dikey kaydırır.
- Form **MinimumSize** düşürüldü: 1000×650.

Her yeniden boyutta `DoResponsiveLayout()` çalışır. Gerekirse eşikleri (yüzde/px) kendine göre ayarlayabilirsin.
