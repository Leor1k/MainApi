# 🌐 OctarineCore API

**OctarineCore API** — это серверная часть проекта [OctarineCore](https://github.com/Leor1k/OctarineCore), реализованная на ASP.NET Core. Отвечает за регистрацию и авторизацию пользователей, чаты, друзей, управление комнатами, а также хранение медиафайлов (например, аватарок) через MinIO.

---

## 📌 Основные функции

- 🔐 Регистрация и подтверждение email
- 🔑 JWT-аутентификация
- 👥 Система друзей
- 💬 Текстовые чаты
- 🧩 Комнаты для голосовой связи
- 🖼 Загрузка аватаров пользователей в MinIO
- 📦 REST API для WPF-клиента

---

## ⚙️ Технологии

- ASP.NET Core 8
- Entity Framework Core
- PostgreSQL
- MinIO (S3-compatible хранилище)
- JWT Authentication
- FluentValidation
---
## 🖼 Работа с MinIO

**MinIO** используется для хранения файлов (например, аватаров). API предоставляет endpoint для загрузки и получения URL файлов.
---
### 🔗 Связь с другими модулями
- 📡 Передаёт пользователей и комнаты в голосовой модуль (https://github.com/Leor1k/VoiceRepeater)
- 🖥 Обслуживает WPF-клиент (OctarineCore UI) (https://github.com/Leor1k/OctarineCore)
---
## 📄 Лицензия
Проект распространяется под лицензией **MIT** — свободное использование в учебных и некоммерческих целях.
---
## 👤 Автор
- [GitHub](https://github.com/Leor1k) 
- [Email](leoriknn@yandex.ru)
