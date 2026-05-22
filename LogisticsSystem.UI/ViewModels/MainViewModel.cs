using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using LogisticsSystem.UI.Models;
using LogisticsSystem.UI.Helpers; // Importujemy naszego Helpera!

namespace LogisticsSystem.UI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged // To jest nasz główny ViewModel, który będzie zarządzał danymi i logiką dla naszego interfejsu użytkownika
    {
        private readonly string _filePath = "magazyn.json";
        private SparePart _selectedPart;

        public ObservableCollection<SparePart> PartsInventory { get; set; } = new ObservableCollection<SparePart>();

        public SparePart SelectedPart
        {
            get => _selectedPart;
            set { _selectedPart = value; OnPropertyChanged(); }
        }

        // --- ZMIENNE DO BINDOWANIA PÓL TEKSTOWYCH Z INTERFEJSU ---
        private string _deliveryAmountText = "0";
        public string DeliveryAmountText { get => _deliveryAmountText; set { _deliveryAmountText = value; OnPropertyChanged(); } }

        private string _issueAmountText = "0";
        public string IssueAmountText { get => _issueAmountText; set { _issueAmountText = value; OnPropertyChanged(); } }

        private string _aisleInput, _rackInput, _shelfInput; // Te trzy pola służą do wprowadzania lokalizacji magazynowej (alejka, regał, półka)
        public string AisleInput { get => _aisleInput; set { _aisleInput = value; OnPropertyChanged(); } }
        public string RackInput { get => _rackInput; set { _rackInput = value; OnPropertyChanged(); } }
        public string ShelfInput { get => _shelfInput; set { _shelfInput = value; OnPropertyChanged(); } }

        private string _newPartName, _newPartCode, _newPartPriceText;
        public string NewPartName { get => _newPartName; set { _newPartName = value; OnPropertyChanged(); } }
        public string NewPartCode { get => _newPartCode; set { _newPartCode = value; OnPropertyChanged(); } }
        public string NewPartPriceText { get => _newPartPriceText; set { _newPartPriceText = value; OnPropertyChanged(); } }

        // --- KOMENDY (COMMANDS) ZAMIAST ZDARZEŃ CLICK ---
        public ICommand ReceiveDeliveryCommand { get; }
        public ICommand IssueFromStockCommand { get; }
        public ICommand ChangeLocationCommand { get; }
        public ICommand ApproveOrderCommand { get; }
        public ICommand AddNewPartCommand { get; }

        public MainViewModel()
        {
            LoadDataFromFile();

            // INICJALIZACJA KOMEND (Z użyciem naszego RelayCommand)
            ReceiveDeliveryCommand = new RelayCommand(ExecuteReceiveDelivery, CanExecutePartOperation);
            IssueFromStockCommand = new RelayCommand(ExecuteIssueFromStock, CanExecutePartOperation);
            ChangeLocationCommand = new RelayCommand(ExecuteChangeLocation, CanExecutePartOperation);
            ApproveOrderCommand = new RelayCommand(ExecuteApproveOrder, CanExecutePartOperation);
            AddNewPartCommand = new RelayCommand(ExecuteAddNewPart);
        }

        // --- LOGIKA BIZNESOWA WYWOŁYWANA PRZEZ KOMENDY ---

        // Metoda sprawdzająca, czy przycisk może być kliknięty (blokuje przycisk, jeśli nie ma wybranej części!)
        private bool CanExecutePartOperation(object parameter) => SelectedPart != null; // To sprawia, że przyciski będą nieaktywne, dopóki użytkownik nie wybierze części z listy

        private void ExecuteReceiveDelivery(object parameter) // Ta metoda jest wywoływana, gdy użytkownik kliknie przycisk "Przyjmij dostawę"
        {
            if (int.TryParse(DeliveryAmountText, out int amount))
            {
                SelectedPart.ReceiveDelivery(amount);
                DeliveryAmountText = "0"; // Czyści pole tekstowe
            }
            else MessageBox.Show("Proszę wpisać prawidłową liczbę całkowitą.");
        }

        private void ExecuteIssueFromStock(object parameter) // Ta metoda jest wywoływana, gdy użytkownik kliknie przycisk "Wydaj z magazynu"
        {
            if (int.TryParse(IssueAmountText, out int amount))
            {
                SelectedPart.UpdateStock(-amount);
                IssueAmountText = "0";
            }
            else MessageBox.Show("Proszę wpisać prawidłową liczbę.");
        }

        private void ExecuteChangeLocation(object parameter)// Ta metoda jest wywoływana, gdy użytkownik kliknie przycisk "Zmień lokalizację"
        {
            SelectedPart.SetStorageLocation(AisleInput, RackInput, ShelfInput);
            AisleInput = RackInput = ShelfInput = string.Empty;
        }

        private void ExecuteApproveOrder(object parameter)// Ta metoda jest wywoływana, gdy użytkownik kliknie przycisk "Zatwierdź zamówienie"
        {
            var result = MessageBox.Show("Zatwierdzić zamówienie?", "Potwierdzenie", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes) SelectedPart.ApproveOrder();
        }

        private void ExecuteAddNewPart(object parameter) // Ta metoda jest wywoływana, gdy użytkownik kliknie przycisk "Dodaj nową część"
        {
            if (string.IsNullOrWhiteSpace(NewPartName) || string.IsNullOrWhiteSpace(NewPartCode))
            {
                MessageBox.Show("Pola nie mogą być puste."); return;
            }
            if (decimal.TryParse(NewPartPriceText, out decimal price))
            {
                PartsInventory.Add(new SparePart(NewPartName, NewPartCode, price));
                NewPartName = NewPartCode = NewPartPriceText = string.Empty;
            }
            else MessageBox.Show("Niepoprawna cena.");
        }

        // --- ZAPIS I ODCZYT PLIKU  ---
        public void SaveDataToFile()
        {
            string jsonString = JsonSerializer.Serialize(PartsInventory, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, jsonString);
        }

        private void LoadDataFromFile() // Ta metoda jest wywoływana w konstruktorze, aby załadować dane z pliku JSON przy starcie aplikacji
        {
            if (File.Exists(_filePath))
            {
                var loaded = JsonSerializer.Deserialize<ObservableCollection<SparePart>>(File.ReadAllText(_filePath));
                if (loaded != null) PartsInventory = loaded;
            }
            else LoadSampleData();
        }

        private void LoadSampleData() // Ta metoda jest wywoływana, jeśli nie ma pliku z danymi - wypełnia magazyn przykładowymi częściami, aby użytkownik miał co zobaczyć i testować
        {
            var p1 = new SparePart("Klocki hamulcowe Bosch", "BOSCH-123", 149.99m); p1.ReceiveDelivery(12); p1.SetStorageLocation("B", "12", "4");
            var p2 = new SparePart("Filtry oleju Filtron", "FIL-999", 35.50m); p2.ReceiveDelivery(5); p2.SetStorageLocation("A", "02", "1");
            PartsInventory.Add(p1); PartsInventory.Add(p2);
        }

        public event PropertyChangedEventHandler PropertyChanged; // To jest standardowy mechanizm powiadamiania interfejsu użytkownika o zmianach w danych (np. gdy zmieni się wybrana część lub zawartość pól tekstowych)
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));// Ta metoda jest wywoływana, gdy chcemy powiadomić interfejs użytkownika, że jakaś właściwość się zmieniła (np. SelectedPart, DeliveryAmountText itp.)
    }
}