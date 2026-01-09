# TiAnomalyInstaller

![image](assets/main.png)

## Описание

Данный инсталлятор служит для упрощения и унификации процесса установки сборок для **S.T.A.L.K.E.R.: Anomaly**.

## Как все работает?

* Для работы программы, на своей стороне, вы дожны поддержать **2 конфига** - *локальный* и *ремоут*:  
  * *локальный* конфиг - в формате `toml`, хранится локально, рядом с инсталлятором, по пути `<текущая директория>\Storage\config.toml`:

    ```toml
    # заголовок
    title = "~ template ~"
    # прямая ссылка на ремоут конфиг
    url = "https://raw.githubusercontent.com/.../RemoteConfig.json"
    # локальная версия
    version = "0.0.0"
    ```

  * *ремоут* конфиг - в формате `json`, хранится удаленно и каждый раз загружается при старте инсталлятора:

    ```json
    {
        "Profile": "G.A.M.M.A.", // название профиля, которое пропишется в ModOrganizer.ini
        "Hash": {
            "ArchiveChecksumsUrl": "https://github.com/.../ArchiveChecksums.7z", // прямая ссылка на архив с хеш-суммой для каждого архива из массива Archives
            "GameChecksumsUrl": ""                                               // в разработке
        },
        "Archives": [
            {
                "Url": "https://disk.yandex.ru/.../aaabbbccc", // поддерживается ссылка с Я.Диска или прямая ссылка до файла
                "FileName": "Archive-1.7z",                    // название архива при локальном сохранении
                "Type": "Vanilla",                             // по факту - папка распаковки (enum в коде) - 2 варианта - Vanilla|Organizer
                "Operations": []                               // в разработке
            },
            {
                "Url": "https://disk.yandex.ru/.../aaabbbccc", // поддерживается ссылка с Я.Диска или прямая ссылка до файла
                "FileName": "Archive-2.7z",                    // название архива при локальном сохранении
                "Type": "Organizer",                           // по факту - папка распаковки (enum в коде) - 2 варианта - Vanilla|Organizer
                "Operations": []                               // в разработке
            }
        ],
        "Version": "1.0.0" // ремоут версия
    }
    ```
