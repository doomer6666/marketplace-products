# Marketplace Products Service
Микросервис управления каталогом товаров для e-commerce платформы.

## Стек
* **Backend:** .NET 8 (C# 12) / ASP.NET Core Web API / gRPC
* **Primary Database (Write):** PostgreSQL + Dapper + FluentMigrator
* **Search Engine (Read):** Elasticsearch 8.17
* **Message Broker:** Kafka (Confluent.Kafka)
* **Caching:** Redis
* **Validation:** FluentValidation
* **Testing:** xUnit, Moq, FluentAssertions, Testcontainers (Postgres, Elastic, Redis, Kafka)

## Структура проекта
* `Domain` - ядро, главные сущности.
* `Infrastructure` - транспортный уровень, взаимодействие с базами данных (Postgres, Elasticsearch, Redis) и брокерами (Kafka).
* `Application` - бизнес-логика. Содержит валидаторы, ДТО, контракты интерфейсов.
* `Api` - внешние интерфесы (REST Controllers, gRPC Endpoints).
* `Migrations` - версионирование схемы БД (FluentMigrator).
* `IntegrationTests` - интеграционные тесты с поднятием инфраструктуры через Docker (Testcontainers).
* `UnitTests` - юнит тесты.

### 1. Подъем инфраструктуры
Убедитесь, что у вас установлен **Docker**.
```bash
docker-compose up -d
```
### 2. Запуск приложения и миграций
Приложение автоматически накатит миграции схемы БД при старте (если не используется CLI).

```bash
dotnet run --project Marketplace.Products.Api
```

### 3(Опционально). Генерация тестовых данных (DevTools)
Чтобы не работать с пустой базой, в сервисе предусмотрен специальный инструмент разработчика.
Откройте Swagger UI по адресу http://localhost:7263/swagger и выполните:

`POST /api/v1/dev-tools/generate-fake-products` - сгенерирует указанное количество товаров.

