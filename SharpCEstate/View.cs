// View.cs
using System;

namespace SharpCEstate
{
    public static class ViewUpdater
    {
        private static bool isForecastShown = false;
        public static void PrepareInterface()
        {
            Console.WriteLine("Interface preparada. A aplicação está pronta para receber comandos.");
            isForecastShown = false; // Resetar o estado cada vez que a interface é preparada
        }

        public static void ShowForecast(float predictedPrice)
        {
            if (!isForecastShown)
            {
                Console.WriteLine($"Previsão de preço exibida: {predictedPrice} €.");
                isForecastShown = true; // Marcar como mostrado para evitar duplicação
            }
        }

        public static void ResetForecastDisplay()
        {
            isForecastShown = false; // Resetar a exibição da previsão para novas interações
        }
    }

    public static class UserInteractionView
    {
        public static void Interact()
        {
            ViewUpdater.ResetForecastDisplay(); // Assegurar que a previsão possa ser mostrada novamente

            Console.WriteLine("Interagindo com o usuário. Digite 'sair' para encerrar ou 'nova' para uma nova previsão.");
            string userInput = Console.ReadLine()?.Trim() ?? string.Empty;
            if (userInput.ToLower() == "nova")
            {
                Console.WriteLine("Por favor, insira os detalhes do imóvel (Área, Localização, Tipo):");
                string[] inputs = Console.ReadLine()?.Split(',') ?? new string[0];
                if (inputs.Length >= 3)
                {
                    try
                    {
                        float area = float.Parse(inputs[0].Trim());
                        string localizacao = inputs[1].Trim();
                        string nome = inputs[2].Trim();

                        // Usar a instância Singleton para prever preço
                        ApplicationController controller = ApplicationController.Instance;
                        controller.RequestPriceForecast(area, localizacao, nome);
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine("Formato inválido. Certifique-se de que a área é um número e os detalhes estão corretos.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao processar a previsão: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("Entrada inválida. Certifique-se de inserir os detalhes corretamente.");
                }
            }
            else if (userInput.ToLower() == "sair")
            {
                Console.WriteLine("Encerrando a aplicação...");
                Environment.Exit(0);
            }
        }
    }
}
