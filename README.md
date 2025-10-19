# SpeechToText
Bu proje, C# WinForms + Whisper.NET + NAudio kullanarak mikrofon ile konuşmayı metne çeviren basit bir uygulamadır.

Özellikler

Mikrofon kaydını alır ve Whisper ile metne dönüştürür.

Türkçe ve İngilizce desteği.

16 kHz mono ses formatı ile uyumlu.

Kaydı başlat/durdur ve sonucu temizle butonları.

Basit ve kullanıcı dostu arayüz.

Gereksinimler

Windows

Visual Studio 2022 veya üstü

.NET 9.0

NuGet paketleri:

NAudio

Whisper.net

Whisper.net.Ggml

Kurulum

Depoyu klonlayın:

git clone <repo-url>


Projeyi Visual Studio ile açın.

NuGet paketlerini yükleyin (Restore).

ggml-small.bin dosyası bin/Debug/net9.0-windows/ klasöründe olmalı. (Program ilk çalıştırmada indiriyor.)

Uygulamayı çalıştırın (F5).

Kullanım

Uygulamayı açın.

Dil seçin (Türkçe veya İngilizce).

"Kayda Başla" butonuna tıklayın ve konuşun.

"Kayda Son Ver" butonuna tıklayın.

Metin otomatik olarak kutuya eklenecek.

Notlar

Whisper sadece 16 kHz mono ses destekler. Kodda bu ayar otomatik yapılmıştır.

Uygulama x64 platformunda çalışmalıdır. 

Geliştiren:
Beyzanur Karademirli
