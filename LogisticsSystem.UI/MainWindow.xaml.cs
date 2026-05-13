using LogisticsSystem.Core;
using System.Windows;

// TEN NAMESPACE JEST KLUCZOWY - łączy kod C# z plikiem XAML
namespace LogisticsSystem.UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            // Ta metoda załaduje interfejs
            InitializeComponent();

            // Tworzymy przykładowy obiekt
            var samplePart = new SparePart("Klocki Hamulcowe Bosch", "0 986 494 000", 185.50m);

            // Symulujemy, że coś już było na magazynie
            samplePart.ReceiveDelivery(0);
           

            // WYZNACZA MIEJSCE W MAGAZYNIE:
            samplePart.SetStorageLocation("B", "12", "4");

            // Ustawiamy DataContext
            this.DataContext = samplePart;

        }
        private void ReceiveDelivery_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is SparePart part)
            {
                if (int.TryParse(DeliveryAmountInput.Text, out int amount))
                {
                    // 1. Dodajemy towar do magazynu
                    part.ReceiveDelivery(amount);

                    // 2. CZYŚCIMY POLE TEKSTOWE - Zabezpieczenie przed podwójnym kliknięciem
                    DeliveryAmountInput.Clear();
                }
                else
                {
                    MessageBox.Show("Proszę wpisać prawidłową liczbę całkowitą.", "Błąd wprowadzania");
                }
            }
        }

        private void IssueFromStock_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is SparePart part)
            {
                if (int.TryParse(DeliveryAmountInput.Text, out int amount))
                {
                    try
                    {
                        // Próbujemy wykonać operację (pamiętamy o minusie, bo wydajemy)
                        part.UpdateStock(-amount);

                        // Jeśli się udało, czyścimy pole
                        DeliveryAmountInput.Clear();
                    }
                    catch (InvalidOperationException ex)
                    {
                        // Jeśli nasz "bezpiecznik" w klasie SparePart zadziałał (brak towaru),
                        // przechwytujemy ten błąd tutaj i wyświetlamy go użytkownikowi.
                        MessageBox.Show(ex.Message, "Błąd magazynowy", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Proszę wpisać prawidłową liczbę.");
                }
            }
        }
        private void ChangeLocation_Click(object sender, RoutedEventArgs e)
        {
            // Wyciągamy nasz obiekt z DataContextu (Pattern Matching)
            if (this.DataContext is SparePart part)
            {
                // Pobieramy wartości wpisane przez operatora
                string aisle = AisleInput.Text;
                string rack = RackInput.Text;
                string shelf = ShelfInput.Text;

                // ZABEZPIECZENIE: Sprawdzamy, czy żadne z pól nie jest puste lub nie składa się z samych spacji
                if (!string.IsNullOrWhiteSpace(aisle) &&
                    !string.IsNullOrWhiteSpace(rack) &&
                    !string.IsNullOrWhiteSpace(shelf))
                {
                    // Wywołujemy naszą metodę z klasy SparePart
                    part.SetStorageLocation(aisle, rack, shelf);

                    // Czyścimy pola po pomyślnej akcji, żeby zachować porządek na ekranie
                    AisleInput.Clear();
                    RackInput.Clear();
                    ShelfInput.Clear();
                }
                else
                {
                    // Jeśli któreś pole jest puste, wyrzucamy ostrzeżenie
                    MessageBox.Show("Proszę wypełnić wszystkie trzy parametry lokalizacji (Alejka, Regał, Półka).", "Brak danych");
                }
            }
        }
        private void ApproveOrder_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is SparePart part)
            {
                // Opcjonalnie: Okienko upewniające się, czy na pewno chcemy wydać pieniądze
                var result = MessageBox.Show("Czy na pewno chcesz zatwierdzić zamówienie u dostawcy?",
                                             "Potwierdzenie kosztów",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    part.ApproveOrder();
                }
            }
        }
    }
}