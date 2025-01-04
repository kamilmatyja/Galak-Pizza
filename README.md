1. Komendy bazodanowe:

a. Usunięcie bazy danych:
dotnet ef database drop

b. Update bazy danych:
dotnet ef database update

c. Dodanie migracji: 
dotnet ef migrations add xxx

d. Usunięcie migracji:
dotnet ef migrations remove

2. Instrukcja:

a. Projekt napisany i testowany w .net v8.0.10/11

b. Projekt pisany na systemie Linux Ubuntu 24.10

c. Projekt pisany w ide JetBrains Rider

d. Baza danych jest wykonana jako SqlLite przez co projekt zawiera plik app.db z bazą danych

e. Wszystkie potrzebne paczki odpowiednio dodane do projektu

3. Baza testowa:

a. Aplikacja posiada plik app.db z uzupełnioną bazą danych do testów

b. Baza testowa posiada konta użytkowników administrator@localhost.pl moderator@localhost.pl analityk@localhost.pl autor@localhost.pl czytelnik@localhost.pl z hasłami zaq1@WSX

c. Baza testowa posiada przykładowe kategorie, podstrony, wyświetlenia, komentarze, oceny

4. Jak testować samodzielnie bez bazy testowej:

a. Trzeba usunąć baze testową

b. Po uruchomieniu aplikacji bez podstrony z linkiem = null zostaniemy automatycznie przekierowani na strone z listą podstron (aby nie rzucać 404)

c. Trzeba się zarejestrować, potwierdzić konto, zalogować (pierwszy zarejestrowany użytkownik tworzy się z rolą Administrator, pozostali z rola Czytelnik)

d. Po zalogowaniu na Administratora (lub Autora) trzeba stworzyć kategorie

e. Po dodaniu kategorii trzeba stworzyć podstrone

f. Po zalogowaniu na Administratora (lub Moderatora) można dodawać komentarze, oceny, wyświetlenia (z poziomu panelu)

g. Po zalogowaniu na jakiegokolwiek użytkownika na wyświetlonej podstronie można dodawać i edytować komentarze i oceny oraz usuwać swoje komentarze

h. Wyjaśnienie szczegółów działania aplikacji jest w dokumentacji technicznej
