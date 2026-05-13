using System.ComponentModel;
using System.Collections.ObjectModel;

namespace LogisticsSystem.Core;


/// <summary>
/// Represents a physical spare part in the logistics system.
/// Reprezentuje fizyczną część zamienną w systemie logistycznym.
/// </summary>
public class SparePart : INotifyPropertyChanged
{
    // Używamy 'Guid' dla unikalności id w systemach rozproszonych
    public Guid Id { get; private set; }

    // 'string' dla nazw; 'init' oznacza, że nazwę nadajemy tylko raz przy tworzeniu
    public string Name { get; init; }

    /// <summary>
    /// Unique code from producer or distributor (e.g. Inter Cars).
    /// Unikalny kod producenta lub dystrybutora (np. Inter Cars).
    /// </summary>
    public string ProducerCode { get; init; }

    // KLUCZOWE: Dla pieniędzy ZAWSZE używamy typu 'decimal', nie 'double' ani 'float'.
    // Double ma błędy zaokrągleń, które w finansach są niedopuszczalne.
    public decimal Price { get; private set; }

    public int StockQuantity { get; private set; }  // Ilość dostępna na magazynie

    // Znak zapytania oznacza, że data może być pusta (null), jeśli nic nie jedzie.
    public DateTime? ExpectedDeliveryDate { get; private set; }

    // Kiedy ostatnio ta część wjechała na magazyn.
    public DateTime? LastDeliveryDate { get; private set; }

    public event PropertyChangedEventHandler? PropertyChanged;


    // Nowa właściwość z domyślną wartością ostrzegawczą
    public string StorageLocation { get; private set; } = "Brak przypisanej lokalizacji";

    // Inteligentna lista przechowująca historię
    // Intelligent list with a history of operations
    public ObservableCollection<string> OperationHistory { get; private set; } = new ObservableCollection<string>();

    // --- PARAMETRY AUTOMATYKI ZAMÓWIEŃ ---
    public int MinimumStockLevel { get; private set; } = 2;
    public int OptimalStockLevel { get; private set; } = 10;

    // Flaga, żeby system nie zamawiał w kółko, jeśli dostawa jest już w drodze
    public bool IsOrderPending { get; private set; } = false;
    // NOWA FLAGA: Czy zamówienie czeka na podpis kierownika?
    public bool RequiresApproval { get; private set; } = false;





    //===================================================// 

    // Konstruktor wymuszający poprawne dane od samego początku
    public SparePart(string name, string producerCode, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nazwa części nie może być pusta.");

        if (price < 0)
            throw new ArgumentException("Cena nie może być ujemna.");

        Id = Guid.NewGuid();
        Name = name;
        ProducerCode = producerCode;
        Price = price;
        StockQuantity = 0;
    }

    //============================================//
    /// <summary>
    /// Updates the stock quantity.
    /// Aktualizuje stan magazynowy.
    /// </summary>
    public void UpdateStock(int amount) //amount może być dodatni (dostawa) lub ujemny (sprzedaż)
    {
        // Logika biznesowa: nie możemy mieć ujemnego stanu na magazynie
        if (StockQuantity + amount < 0)
            throw new InvalidOperationException("Niewystarczająca ilość na magazynie.");

        StockQuantity += amount;
        OnPropertyChanged(nameof(StockQuantity));

        string action = amount > 0 ? "Przyjęto" : "Wydano";
        LogOperation($"{action} {Math.Abs(amount)} szt. (Stan: {StockQuantity})");

        // Sprawdzamy poziom krytyczny na magazynie i ewentualnie generujemy zamówienie
        // (Włącza się tylko wtedy, gdy stan jest niski i nie ma jeszcze aktywnego zamówienia)
        // Zgłaszamy zapotrzebowanie, ale nie kupujemy!
        if (StockQuantity <= MinimumStockLevel && !IsOrderPending && !RequiresApproval)
        {
            RequiresApproval = true;
            OnPropertyChanged(nameof(RequiresApproval));
            LogOperation($"[ALERT] Osiągnięto minimum. Zamówienie czeka na zatwierdzenie.");
        }
    }

    /// <summary>
    /// Rejestruje przyjęcie nowej dostawy.
    /// </summary>
    public void ReceiveDelivery(int amount)
    {
        UpdateStock(amount);
        LastDeliveryDate = DateTime.Now;
        ExpectedDeliveryDate = null;

        OnPropertyChanged(nameof(LastDeliveryDate));
        // RESETUJEMY ALARM: Towar dojechał, system może znowu zasnąć (AUTOMATYCZNE ZAMÓWIENIA)
        IsOrderPending = false;
        RequiresApproval = false;

        //Trigery do odświeżenia ekranów w WPF
        OnPropertyChanged(nameof(RequiresApproval));
        OnPropertyChanged(nameof(LastDeliveryDate));
        OnPropertyChanged(nameof(ExpectedDeliveryDate));
        OnPropertyChanged(nameof(IsOrderPending));
    }
    /// <summary>
    /// Planuje nową dostawę na konkretny termin.
    /// </summary>
    public void ScheduleDelivery(DateTime deliveryDate)
    {
        if (deliveryDate < DateTime.Now)
            throw new ArgumentException("Nie można zaplanować dostawy na przeszłość.");

        ExpectedDeliveryDate = deliveryDate;
    }
    protected void OnPropertyChanged(string propertyName) // Metoda pomocnicza do wywoływania zdarzenia PropertyChanged
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Przypisuje dokładną lokalizację fizyczną w magazynie.
    /// </summary>
    public void SetStorageLocation(string aisle, string rack, string shelf)
    {
        // Formatujemy ciąg znaków 
        StorageLocation = $"Alejka {aisle}, Regał {rack}, Półka {shelf}";
               
        OnPropertyChanged(nameof(StorageLocation));

        LogOperation($"Zmiana lokalizacji na: {StorageLocation}");
    }

    // Prywatna metoda do równego formatowania wpisów
    private void LogOperation(string operationDetails)
    {
        // Pobieramy aktualny czas z dokładnością do sekund
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        OperationHistory.Insert(0, $"[{timestamp}] {operationDetails}"); // Insert(0, ...) dodaje nowy wpis zawsze na samą górę listy
    }

    // NOWA METODA: Operator ręcznie zatwierdza zamówienie
    public void ApproveOrder()
    {
        if (RequiresApproval)
        {
            RequiresApproval = false;
            IsOrderPending = true;
            ExpectedDeliveryDate = DateTime.Now.AddDays(2); // Przewidywany czas dostawy po akceptacji

            int amountToOrder = OptimalStockLevel - StockQuantity;

            // Odświeżamy ekrany
            OnPropertyChanged(nameof(RequiresApproval));
            OnPropertyChanged(nameof(IsOrderPending));
            OnPropertyChanged(nameof(ExpectedDeliveryDate));

            LogOperation($"[ZATWIERDZONO] Wysłano zamówienie na {amountToOrder} szt.");
        }
    }
}