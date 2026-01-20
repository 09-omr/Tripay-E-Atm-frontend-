# 💳 Dijital Ödeme Sistemleri

Bu proje, **çok katmanlı servis odaklı mimari (SOA)** kullanılarak geliştirilmiş bir
**dijital cüzdan / ödeme sistemi** uygulamasıdır.

Proje kapsamında **REST, gRPC ve SOAP** servisleri birlikte kullanılmış,
ileri seviye **veritabanı tasarımı**, **yetkilendirme** ve **backend mimarisi**
uygulanmıştır.

---

## 🧠 Genel Mimari

- 6 Katmanlı SOA mimarisi
- REST API (Node.js + Express)
- JWT Authentication
- gRPC servisleri
- SOAP servisleri
- PostgreSQL ilişkisel veritabanı

---

## 🗄️ Veritabanı Tasarımı

- RDBMS: **PostgreSQL**
- En az **6 farklı varlık (entity)** kullanımı
- Normalizasyon kurallarına uygun tasarım
- Veri bütünlüğü için:
  - Primary Key
  - Foreign Key
  - Unique Constraint
  - Check Constraint  
  (en az 3 farklı türde, toplam 5 adet)
- Performans için indeksleme stratejileri
- En az:
  - **5 View**
  - **2 Stored Procedure**
  - **2 User Defined Function**
- Rol bazlı yetkilendirme
- Veri maskeleme ve erişim kısıtları

---

##🧑‍💻 Web Uygulaması (MVC)

-En az 5 farklı Controller
-Her Controller’da 3 farklı Action
-PartialView / ViewComponent kullanımı
-Dinamik View yapısı
-Ortak Layout (en az 3 View’da kullanılmış)
-CRUD işlemleri
-En az 2 farklı kullanıcı tipi
-Rol bazlı içerik gösterimi
-ViewBag / ViewData / TempData ile veri aktarımı

---

## 🔌 Servisler

### 🌐 REST API
- Express.js
- JWT Authentication
- CRUD işlemleri
- Rol bazlı yetkilendirme

### ⚡ gRPC Servisi

WalletService
Proto dosyası: proto/wallet.proto
Desteklenen metotlar:
-GetAccountBalance
-Deposit
Çalıştırma:
npm run grpc-server

###🧼 SOAP Servisi

WalletService
Desteklenen operasyon:
GetAccountBalance(accountId)
WSDL:
http://localhost:3001/wsdl
Çalıştırma:
npm run soap-server


###⚙️ ÇALIŞTIRMA

Komutlar:
  npm install
  npm run dev         -> REST API (Express, JWT Auth)
  npm run grpc-server -> gRPC sunucusu (WalletService)
  npm run soap-server -> SOAP sunucusu (WalletService)

gRPC:
  - proto/wallet.proto dosyasindaki WalletService:
    * GetAccountBalance
    * Deposit

SOAP:
  - Tek operasyon: GetAccountBalance(accountId)
  - WSDL: http://localhost:3001/wsdl
