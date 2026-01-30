# TiAnomalyInstaller

![image](assets/main.png)

## Описание

Данный инсталлятор служит для упрощения и унификации процесса установки сборок для **S.T.A.L.K.E.R.: Anomaly**.

## Локальное хранилище

Для работы инсталлятора рядом с его исполняемым файлом должна быть создана папка `Storage`.

В папке `Storage` должен находиться файл `storage.json` со следующим содержимым:

```json
{
  "ProfileUrl": "https://..."
}
```

- `ProfileUrl` — прямая ссылка на удалённый конфигурационный файл (профиль), который будет использоваться инсталлятором.

## Удаленная конфигурация

---

```yaml
# Версия схемы (для валидации и совместимости)
schema_version: 1.0

# Метаданные пакета
metadata:
  title: "~ Template ~"      # Отображаемое название
  profile: Default            # Профиль / режим установки
  latest_version: 0.2.1       # Актуальная версия пакета

# Визуальные параметры (UI)
visual:
  background_image: null      # URL / путь к изображению или null

# Размеры (для отображения пользователю)
size:
  download_bytes: 13519516678 # Общий размер загрузки (байты)
  install_bytes: 26446854973  # Размер после установки (байты)

# Архивы установки и обновлений
archives:

  # Архивы для чистой установки
  install:
    - url: https://...                    # Ссылка на архив
      file_name: Vanilla.7z               # Имя файла после загрузки
      extract_to_folder: Vanilla          # Папка распаковки
      version: 0.2.0                      # Версия архива
      checksum:
        algorithm: MD5
        value: 1036B7629878AEAC8A102E9918A089EB
    - url: https://...
      file_name: Organizer.7z
      extract_to_folder: Organizer
      version: 0.2.0
      checksum:
        algorithm: MD5
        value: 1036B7629878AEAC8A102E9918A089EB

  # Патчи для обновления между версиями
  patch:
    - url: https://...
      file_name: Patch-1.7z
      extract_to_folder: Organizer        # Куда применяется патч
      patch:
        from_version: 0.2.0               # Версия-источник
        to_version: 0.2.1                 # Целевая версия
      checksum:
        algorithm: MD5
        value: E21CB48DD32B9F46F13E1E2DE4C8CA1C
```
