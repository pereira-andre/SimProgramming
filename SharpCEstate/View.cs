/*
** ficheiro: View.cs
**
** UC: 21179 - LDS @ UAb
**
** Alunos: 
** 2202880 - Andre Pereira
** 2203127 - Mario Prazeres
** 2204349 - Ruben Nunes
** 2203141 - Luciano Araujo
** 2201058 - Carla Campanico
*/

using System;
using System.Threading.Tasks;

namespace SharpCEstate
{
    // Classe responsável pela interação com o usuário
    public static class UserInteractionView
    {
        // Método para exibir o título do menu estilizado
        public static void ShowMenuTitle()
        {
            // ASCII art para o título "SHARPCESTATE"
            Console.WriteLine("╔══╗╔╗╔╗╔══╗╔═══╗╔═══╗╔══╗╔═══╗╔══╗╔════╗╔══╗╔════╗╔═══╗");
            Console.WriteLine("║╔═╝║║║║║╔╗║║╔═╗║║╔═╗║║╔═╝║╔══╝║╔═╝╚═╗╔═╝║╔╗║╚═╗╔═╝║╔══╝");
            Console.WriteLine("║╚═╗║╚╝║║╚╝║║╚═╝║║╚═╝║║║──║╚══╗║╚═╗──║║──║╚╝║──║║──║╚══╗");
            Console.WriteLine("╚═╗║║╔╗║║╔╗║║╔╗╔╝║╔══╝║║──║╔══╝╚═╗║──║║──║╔╗║──║║──║╔══╝");
            Console.WriteLine("╔═╝║║║║║║║║║║║║║─║║───║╚═╗║╚══╗╔═╝║──║║──║║║║──║║──║╚══╗");
            Console.WriteLine("╚══╝╚╝╚╝╚╝╚╝╚╝╚╝─╚╝───╚══╝╚═══╝╚══╝──╚╝──╚╝╚╝──╚╝──╚═══╝");
        }

        // Método para interação com o usuário
        public static void Interact()
        {
            while (true)
            {
                Console.WriteLine("\nEscolha uma opção:");
                Console.WriteLine("1. Nova previsão de preço");
                Console.WriteLine("2. Gerar relatório por distrito");
                Console.WriteLine("3. Sair");
                Console.Write("Digite o número da opção desejada: ");
                string userInput = Console.ReadLine()?.Trim() ?? string.Empty;

                switch (userInput)
                {
                    case "1":
                        Console.WriteLine("Por favor, insira os detalhes do imóvel (Área, Localização, Tipo):");
                        var inputs = Console.ReadLine()?.Split(',') ?? new string[0];
                        Task.Run(() => ApplicationController.Instance.HandleUserRequestAsync(inputs)).Wait();
                        break;
                    case "2":
                        Console.WriteLine("Por favor, insira a área do imóvel e o tipo:");
                        var reportInputs = Console.ReadLine()?.Split(',') ?? new string[0];
                        if (reportInputs.Length >= 2)
                        {
                            float area = float.Parse(reportInputs[0].Trim());
                            string type = reportInputs[1].Trim();
                            Task.Run(() => ApplicationController.Instance.GeneratePriceReportAsync(area, type)).Wait();
                        }
                        else
                        {
                            Console.WriteLine("Entrada inválida. Por favor, forneça a área e o tipo do imóvel.");
                        }
                        break;
                    case "3":
                        Console.WriteLine("Encerrando a aplicação...");
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Opção inválida. Por favor, escolha entre as opções disponíveis.");
                        break;
                }
            }
        }
    }

    // Classe responsável por atualizar a visualização
    public static class ViewUpdater
    {
        private static bool isForecastShown = false;

        // Método para preparar a interface
        public static void PrepareInterface()
        {
            Console.WriteLine("Interface preparada. A aplicação está pronta para receber comandos.");
            isForecastShown = false;
        }

        // Método para exibir a previsão de preço
        public static void ShowForecast(float predictedPrice)
        {
            if (!isForecastShown)
            {
                Console.WriteLine($"Previsão de preço exibida: {predictedPrice} €.");
                isForecastShown = true;
            }
        }

        // Método para redefinir a exibição da previsão
        public static void ResetForecastDisplay()
        {
            isForecastShown = false;
        }
    }
}
