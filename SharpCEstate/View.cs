namespace SharpCEstate
{
    public static class ViewUpdater
    {
        public static void PrepareInterface()
        {
            // Logica para preparar a interface
            Console.WriteLine("Interface preparada para receber dados.");
        }

        public static void ShowForecast(float predictedPrice)
        {
            // Lógica para mostrar a previsão
            Console.WriteLine($"Previsão de preço exibida: {predictedPrice}.");
        }
    }

    public static class UserInteractionView
    {
        public static void Interact()
        {
            // Logica para interagir com o usuario
            Console.WriteLine("Interagindo com o usuário para atualizações ou novas consultas.");
        }
    }
}

