using LogisticsSystem.Core; // Mówimy testom, gdzie szukać naszego głównego kodu
using Xunit;

namespace LogisticsSystem.Core.Tests;

public class SparePartTests
{
    // [Fact] to znacznik (atrybut) mówiący środowisku: "To jest test do uruchomienia"
    [Fact]
    public void Constructor_WithValidData_CreatesSparePart() //metoda testowa, nazwa mówi, co testujemy i jaki wynik oczekujemy,
                                                             //w tym przypadku: "Konstruktor z poprawnymi danymi tworzy część zamienną"
    {
        // Wzorzec AAA (Arrange, Act, Assert) - złoty standard testowania

        // 1. ARRANGE (Przygotowanie) - szykujemy dane testowe
        string name = "Filtr oleju";
        string producerCode = "FIL-123";
        decimal price = 45.50m; // 'm' na końcu mówi kompilatorowi, że to typ 'decimal'

        // 2. ACT (Wykonanie) - uruchamiamy nasz kod, wpisujemy dane do konstruktora, tworzymy obiekt
        var part = new SparePart(name, producerCode, price);

        // 3. ASSERT (Sprawdzenie) - weryfikujemy, czy zadziałała poprawnie,
        // porównujemy oczekiwane wartości z tym, co mamy w obiekcie
        Assert.Equal(name, part.Name);
        Assert.Equal(price, part.Price);
        Assert.Equal(0, part.StockQuantity); // Domyślny stan magazynowy powinien wynosić 0
    }

    [Fact]
    public void Constructor_WithNegativePrice_ThrowsException()
    {
        // Arrange
        string name = "Tarcza hamulcowa";
        string producerCode = "BRK-99";
        decimal negativePrice = -10.00m;

        // Act & Assert (Sprawdzamy, czy program rzuci błędem, tak jak to zaprojektowaliśmy)
        Assert.Throws<ArgumentException>(() => new SparePart(name, producerCode, negativePrice));
    }
    [Fact]
    public void UpdateStock_AddsAndSubtractsAmountCorrectly()
    {
        // Arrange
        var part = new SparePart("Klocki hamulcowe", "BRK-01", 120.00m);

        // Act
        part.UpdateStock(10); // Dodajemy 10 sztuk z dostawy
        part.UpdateStock(-3); // Wydajemy 3 sztuki mechanikowi

        // Assert
        Assert.Equal(7, part.StockQuantity); // Na półce powinno zostać dokładnie 7
    }

    [Fact]
    public void UpdateStock_BelowZero_ThrowsException()
    {
        // Arrange
        var part = new SparePart("Klocki hamulcowe", "BRK-01", 120.00m);
        part.UpdateStock(5); // Mamy 5 sztuk na stanie

        // Act & Assert
        // Próbujemy wydać 10 sztuk, mając tylko 5. Spodziewamy się błędu (InvalidOperationException).
        Assert.Throws<InvalidOperationException>(() => part.UpdateStock(-10));
    }
}