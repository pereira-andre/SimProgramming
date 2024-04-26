// View.cs
using System;

namespace SharpCEstate
{
    public static class ViewUpdater
    {
        public static void PrepareInterface()
        {
            Console.WriteLine("Interface preparada. A aplicação está pronta para receber comandos.");
        }

        public static void ShowForecast(float predictedPrice)
        {
            Console.WriteLine($"Previsão de preço exibida: {predictedPrice} €.");
        }
    }

    public static class UserInteractionView
    {
        public static void Interact()
        {
            Console.WriteLine("Interagindo com o usuário. Digite 'sair' para encerrar ou 'nova' para uma nova previsão.");
            string userInput = Console.ReadLine()?.Trim() ?? string.Empty;
            if (userInput.ToLower() == "nova")
            {
                Console.WriteLine("Por favor, insira os detalhes do imóvel (Área, Localização, Tipo):");
                string[] inputs = Console.ReadLine()?.Split(',') ?? new string[0];
                if (inputs.Length >= 3)
                {
                    float area = float.Parse(inputs[0].Trim());
                    string localizacao = inputs[1].Trim();
                    string nome = inputs[2].Trim();

                    var controller = new ApplicationController();
                    controller.RequestPriceForecast(area, localizacao, nome);
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
