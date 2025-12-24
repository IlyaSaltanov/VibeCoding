import random
import json
import os
import subprocess
import sys
from datetime import datetime

class RPGCharacterForge:
    def __init__(self, use_ai=False, ai_model="llama3.2"):
        """
        Инициализация генератора.
        use_ai: Пытаться ли использовать локальную LLM через Ollama.
        ai_model: Название модели для Ollama.
        """
        self.use_ai = use_ai
        self.ai_model = ai_model
        self.character = {}
        self.setup_data()

    def setup_data(self):
        """Загрузка шаблонных данных из файлов или создание встроенных наборов."""
        # В реальном проекте лучше вынести в отдельные JSON-файлы
        self.races = ["Человек", "Эльф", "Дварф", "Халфлинг", "Гном", "Орк", "Тифлинг", "Драконорождённый"]
        self.classes = ["Воин", "Волшебник", "Плут", "Жрец", "Следопыт", "Паладин", "Варвар", "Бард"]
        self.origins = [
            "Вы выросли в шумном портовом городе.",
            "Ваше детство прошло в тихой лесной деревне.",
            "Вы были уличным сиротой в огромной столице.",
            "Вы принадлежите к знатному, но обедневшему роду.",
            "Вы приехали из далёкой, экзотической страны.",
            "Вы выросли среди учёных в великой библиотеке.",
        ]
        self.events = [
            "Потеряли близкого родственника из-за таинственной болезни.",
            "Нашли древний артефакт, который до сих пор иногда говорит с вами.",
            "Спасли важного человека, чем заработали могущественного друга или врага.",
            "Были преданы тем, кому доверяли больше всего.",
            "Стали свидетелем события, которое должно было остаться в тайне.",
            "Вашу семью изгнали из родного дома по ложному обвинению.",
        ]
        self.traits_good = ["Храбрый", "Сообразительный", "Добрый", "Верный", "Красноречивый", "Терпеливый", "Любопытный"]
        self.traits_bad = ["Вспыльчивый", "Доверчивый", "Жадный", "Трусливый", "Надменный", "Циничный", "Ленивый"]
        self.motivations = ["Месть", "Обретение знаний", "Защита слабых", "Стать легендой", "Разбогатеть", "Искупить вину", "Найти пропавшего человека"]
        self.items = ["Потёртый медный медальон", "Кинжал с инкрустированной рукоятью", "Дневник предка", "Таинственный ключ", "Карта сокровищ", "Письмо от неизвестного поклонника"]

    def roll_stats(self, method="4d6"):
        """Генерация характеристик (Сила, Ловкость, и т.д.)."""
        if method == "4d6":
            stats = []
            for _ in range(6):
                rolls = sorted([random.randint(1, 6) for _ in range(4)])
                stats.append(sum(rolls[1:])) # Отбрасываем наименьший
            random.shuffle(stats)
            attributes = ["Сила", "Ловкость", "Телосложение", "Интеллект", "Мудрость", "Харизма"]
            self.character["stats"] = dict(zip(attributes, stats))
        return self.character["stats"]

    def generate_basic_info(self):
        """Генерация базовой информации о персонаже."""
        self.character["name"] = f"{random.choice(['Альберт', 'Сигрид', 'Морган', 'Лирия', 'Тарик', 'Элоди'])} {random.choice(['Сторм', 'Стальной', 'Из Теней', 'Мудрый', 'Красный', 'Быстрая Река'])}"
        self.character["race"] = random.choice(self.races)
        self.character["class"] = random.choice(self.classes)
        self.character["level"] = random.randint(1, 5)

    def generate_background_with_ai(self):
        """Попытка сгенерировать биографию с помощью локальной LLM (Ollama)."""
        if not self.use_ai:
            return None

        # Создаем промпт для ИИ на основе уже сгенерированных базовых данных
        prompt = f"""
        Создай короткую (3-4 предложения), яркую и живую биографию для персонажа настольной RPG.
        Имя: {self.character['name']}
        Раса: {self.character['race']}
        Класс: {self.character['class']}
        Происхождение: {random.choice(self.origins)}
        Ключевое событие: {random.choice(self.events)}
        Интересный предмет: {random.choice(self.items)}
        Стиль: темное фэнтези, лаконично, атмосферно.
        """
        try:
            # Вызов Ollama через API (убедитесь, что ollama сервер запущен)
            result = subprocess.run(
                ["ollama", "run", self.ai_model, prompt],
                capture_output=True,
                text=True,
                timeout=30  # Таймаут на случай, если модель долго думает
            )
            if result.returncode == 0:
                return result.stdout.strip()
            else:
                print(f"Ошибка Ollama: {result.stderr}")
                return None
        except FileNotFoundError:
            print("Ollama не найден. Убедитесь, что он установлен и добавлен в PATH.")
            self.use_ai = False
            return None
        except subprocess.TimeoutExpired:
            print("Таймаут запроса к ИИ. Использую шаблонную биографию.")
            return None

    def generate_background_template(self):
        """Генерация биографии из шаблонных частей."""
        origin = random.choice(self.origins)
        event = random.choice(self.events)
        goal = random.choice(self.motivations)
        trait1, trait2 = random.sample(self.traits_good, 2)
        flaw = random.choice(self.traits_bad)
        item = random.choice(self.items)

        background = f"{origin} {event} Это привело вас к цели: {goal}. "
        background += f"Вы {trait1.lower()} и {trait2.lower()}, но также {flaw.lower()}. "
        background += f"Ваша самая ценная вещь — {item.lower()}."

        self.character["background"] = background
        self.character["motivation"] = goal
        self.character["traits"] = [trait1, trait2, f"Слабость: {flaw}"]
        self.character["item"] = item

    def forge_character(self):
        """Основной метод создания персонажа."""
        print("Куёт нового персонажа...")
        self.generate_basic_info()
        self.roll_stats()

        # Пытаемся использовать ИИ для биографии
        ai_background = None
        if self.use_ai:
            print("Запрос к локальной ИИ для создания истории...")
            ai_background = self.generate_background_with_ai()

        if ai_background:
            self.character["background"] = ai_background
            self.character["motivation"] = random.choice(self.motivations)
            self.character["traits"] = random.sample(self.traits_good, 2) + [f"Слабость: {random.choice(self.traits_bad)}"]
            self.character["item"] = random.choice(self.items)
            self.character["source"] = "AI (Ollama)"
        else:
            self.generate_background_template()
            self.character["source"] = "Шаблонный генератор"

        self.character["created"] = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        print("Персонаж создан!")
        return self.character

    def display_character(self):
        """Красивый вывод персонажа в консоль."""
        print("\n" + "="*50)
        print("ВАШ НОВЫЙ ПЕРСОНАЖ")
        print("="*50)
        print(f"Имя: {self.character.get('name', 'N/A')}")
        print(f"Раса: {self.character.get('race', 'N/A')} | Класс: {self.character.get('class', 'N/A')} | Ур. {self.character.get('level', 1)}")
        print("\nХАРАКТЕРИСТИКИ:")
        if 'stats' in self.character:
            for attr, value in self.character['stats'].items():
                print(f"  {attr}: {value:2d} ({'+' + str((value-10)//2) if value >=10 else (value-10)//2})")
        print("\nИСТОРИЯ И ЛИЧНОСТЬ:")
        print(f"  {self.character.get('background', 'N/A')}")
        print(f"\n  Черты: {', '.join(self.character.get('traits', []))}")
        print(f"  Цель: {self.character.get('motivation', 'N/A')}")
        print(f"  Значимый предмет: {self.character.get('item', 'N/A')}")
        print(f"\nСоздан: {self.character.get('created', 'N/A')} | Источник: {self.character.get('source', 'N/A')}")
        print("="*50)

    def save_character(self, format="txt"):
        """Сохранение персонажа в файл."""
        filename = f"character_{self.character.get('name', 'unknown').replace(' ', '_')}.{format}"
        try:
            if format.lower() == "json":
                with open(filename, 'w', encoding='utf-8') as f:
                    json.dump(self.character, f, ensure_ascii=False, indent=2)
            else: # txt или md
                with open(filename, 'w', encoding='utf-8') as f:
                    f.write(f"# Персонаж: {self.character.get('name')}\n\n")
                    f.write(f"**Раса/Класс:** {self.character.get('race')} - {self.character.get('class')} (Ур. {self.character.get('level')})\n\n")
                    f.write("## Характеристики\n")
                    for attr, value in self.character.get('stats', {}).items():
                        f.write(f"- {attr}: {value}\n")
                    f.write("\n## История\n")
                    f.write(f"{self.character.get('background')}\n\n")
                    f.write(f"**Цель:** {self.character.get('motivation')}\n")
                    f.write(f"**Черты:** {', '.join(self.character.get('traits', []))}\n")
                    f.write(f"**Предмет:** {self.character.get('item')}\n")
            print(f"Персонаж сохранён в файл: {filename}")
        except Exception as e:
            print(f"Ошибка при сохранении: {e}")

def main():
    """Основная функция для взаимодействия с пользователем."""
    print("Добро пожаловать в RPG Character Forge!")
    print("Проверяю наличие Ollama для улучшенной генерации истории...")

    # Проверяем, установлен ли Ollama
    use_ai = False
    try:
        subprocess.run(["ollama", "--version"], capture_output=True, check=True)
        print("Ollama найден! Использовать ИИ для создания биографии? (y/n): ")
        if input().lower().startswith('y'):
            use_ai = True
            print("Доступные модели (пример): llama3.2, mistral, gemma")
            print("Введите имя модели (по умолчанию llama3.2): ")
            model = input().strip()
            ai_model = model if model else "llama3.2"
        else:
            ai_model = "llama3.2"
    except (FileNotFoundError, subprocess.CalledProcessError):
        print("Ollama не найден. Будет использован шаблонный генератор.")

    # Создаём генератор
    forge = RPGCharacterForge(use_ai=use_ai, ai_model=ai_model if use_ai else "llama3.2")

    while True:
        print("\nМеню:")
        print("1. Создать нового персонажа")
        print("2. Показать текущего персонажа")
        print("3. Сохранить персонажа в файл (TXT)")
        print("4. Сохранить персонажа в файл (JSON)")
        print("5. Выйти")
        choice = input("Выберите действие: ")

        if choice == "1":
            character = forge.forge_character()
            forge.display_character()
        elif choice == "2":
            if forge.character:
                forge.display_character()
            else:
                print("Сначала создайте персонажа.")
        elif choice == "3":
            if forge.character:
                forge.save_character(format="txt")
            else:
                print("Сначала создайте персонажа.")
        elif choice == "4":
            if forge.character:
                forge.save_character(format="json")
            else:
                print("Сначала создайте персонажа.")
        elif choice == "5":
            print("До новых встреч в приключениях!")
            break
        else:
            print("Неверный ввод.")

if __name__ == "__main__":
    main()