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
}