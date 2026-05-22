using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace LogisticsSystem.UI.Models
{
    public class SparePart : INotifyPropertyChanged
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Name { get; private set; }
        public string ProducerCode { get; private set; }
        public decimal Price { get; private set; }

        [JsonInclude]
        public string StorageLocation { get; private set; } = "Brak przypisanej lokalizacji";

        [JsonInclude]
        public int StockQuantity { get; private set; }

        [JsonInclude]
        public DateTime? LastDeliveryDate { get; private set; }

        [JsonInclude]
        public DateTime? ExpectedDeliveryDate { get; private set; }

        [JsonInclude]
        public bool IsOrderPending { get; private set; }

        [JsonInclude]
        public bool RequiresApproval { get; private set; }

        [JsonInclude]
        public ObservableCollection<string> OperationHistory { get; private set; } = new ObservableCollection<string>();

        public int MinimumStockLevel { get; private set; } = 3;
        public int OptimalStockLevel { get; private set; } = 15;

        public SparePart(string name, string producerCode, decimal price)
        {
            Name = name;
            ProducerCode = producerCode;
            Price = price;
        }

        public void ReceiveDelivery(int amount) // Przyjęcie dostawy i aktualizacja stanu magazynowego
        {
            StockQuantity += amount;
            LastDeliveryDate = DateTime.Now;
            IsOrderPending = false;
            RequiresApproval = false;
            LogOperation($"Przyjęto {amount} szt. (Stan: {StockQuantity})");
            OnPropertyChanged(nameof(StockQuantity));
            OnPropertyChanged(nameof(LastDeliveryDate));
            OnPropertyChanged(nameof(IsOrderPending));
            OnPropertyChanged(nameof(RequiresApproval));
        }

        public void UpdateStock(int amountChange) // Aktualizacja stanu magazynowego (np. po wydaniu części)
        {
            StockQuantity += amountChange;
            LogOperation($"Wydano {Math.Abs(amountChange)} szt. (Stan: {StockQuantity})");
            OnPropertyChanged(nameof(StockQuantity));

            if (StockQuantity <= MinimumStockLevel && !IsOrderPending && !RequiresApproval) // Sprawdzenie, czy należy złożyć zamówienie
            {
                RequiresApproval = true;
                OnPropertyChanged(nameof(RequiresApproval));
                LogOperation($"[ALERT] Osiągnięto minimum. Zamówienie czeka na zatwierdzenie.");
            }
        }

        public void ApproveOrder() // Zatwierdzenie zamówienia i wysłanie go do realizacji
        {
            if (RequiresApproval)
            {
                RequiresApproval = false;
                IsOrderPending = true;
                ExpectedDeliveryDate = DateTime.Now.AddDays(2);
                int amountToOrder = OptimalStockLevel - StockQuantity;

                OnPropertyChanged(nameof(RequiresApproval));
                OnPropertyChanged(nameof(IsOrderPending));
                OnPropertyChanged(nameof(ExpectedDeliveryDate));

                LogOperation($"[ZATWIERDZONO] Wysłano zamówienie na {amountToOrder} szt.");// Tutaj można dodać logikę integracji z systemem zamówień
            }
        }

        public void SetStorageLocation(string aisle, string rack, string shelf)// Ustawienie lokalizacji magazynowej i logowanie tej operacji
        {
            StorageLocation = $"Alejka {aisle}, Regał {rack}, Półka {shelf}";
            LogOperation($"Zmiana lokalizacji na: {StorageLocation}");
            OnPropertyChanged(nameof(StorageLocation));
        }

        private void LogOperation(string message)// Dodanie wpisu do historii operacji z aktualnym czasem
        {
            string logEntry = $"[{DateTime.Now:HH:mm:ss}] {message}";
            OperationHistory.Insert(0, logEntry);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)// Metoda pomocnicza do wywoływania zdarzenia PropertyChanged
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}