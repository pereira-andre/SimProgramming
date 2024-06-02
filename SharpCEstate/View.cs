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
using System.IO;
using System.Diagnostics;

namespace SharpCEstate
{
    public static class UserInteractionView
    {
        // Exibe o título do menu principal
        public static void ShowMenuTitle()
        {
            Console.WriteLine("╔══╗╔╗╔╗╔══╗╔═══╗╔═══╗╔══╗╔═══╗╔══╗╔════╗╔══╗╔════╗╔═══╗");
            Console.WriteLine("║╔═╝║║║║║╔╗║║╔═╗║║╔═╗║║╔═╝║╔══╝║╔═╝╚═╗╔═╝║╔╗║╚═╗╔═╝║╔══╝");
            Console.WriteLine("║╚═╗║╚╝║║╚╝║║╚═╝║║╚═╝║║║──║╚══╗║╚═╗──║║──║╚╝║──║║──║╚══╗");
            Console.WriteLine("╚═╗║║╔╗║║╔╗║║╔╗╔╝║╔══╝║║──║╔══╝╚═╗║──║║──║╔╗║──║║──║╔══╝");
            Console.WriteLine("╔═╝║║║║║║║║║║║║║─║║───║╚═╗║╚══╗╔═╝║──║║──║║║║──║║──║╚══╗");
            Console.WriteLine("╚══╝╚╝╚╝╚╝╚╝╚╝╚╝─╚╝───╚══╝╚═══╝╚══╝──╚╝──╚╝╚╝──╚╝──╚═══╝");
        }

        // Exibe o menu principal e lida com as interações do usuário
        public static void ShowMenu()
        {
            while (true)
            {
                Console.WriteLine("\nEscolha uma opção:");
                Console.WriteLine("1. Nova previsão de preço");
                Console.WriteLine("2. Gerar relatório por distrito");
                Console.WriteLine("3. Gerar relatório com valor máximo");
                Console.WriteLine("4. Manutenção");
                Console.WriteLine("0. Sair");
                Console.Write("Digite o número da opção desejada: ");
                string userInput = Console.ReadLine()?.Trim() ?? string.Empty;

                ApplicationController.Instance.HandleUserInteractions(userInput).Wait();
            }
        }

        // Exibe a previsão de preço
        public static void ShowForecast(float predictedPrice)
        {
            Console.WriteLine($"Previsão de preço exibida: {predictedPrice} €.");
        }

        // Exibe o caminho do relatório gerado
        public static void ShowReport(string reportPath)
        {
            Console.WriteLine($"Relatório criado com sucesso: {reportPath}");
        }

        // Prepara a interface para interação com o usuário
        public static void PrepareInterface()
        {
            Console.WriteLine("Interface preparada. A aplicação está pronta para receber comandos.");
        }

        // Exibe o menu de manutenção e lida com as opções de manutenção
        public static void ShowMaintenanceMenu()
        {
            bool returnToMainMenu = false;

            do
            {
                Console.WriteLine("\nEscolha uma opção de manutenção:");
                Console.WriteLine("1. Executar checklist de testes");
                Console.WriteLine("2. Verificar catálogo de erros");
                Console.WriteLine("3. Listar localização dos ficheiros");
                Console.WriteLine("4. Mostrar componentes do programa");
                Console.WriteLine("5. Estrutura detalhada do programa");
                Console.WriteLine("6. Verificar delegados");
                Console.WriteLine("7. Mostrar Diagrama UML do Programa");
                Console.WriteLine("8. Mostrar Diagrama UML de Captação de Erros");
                Console.WriteLine("9. Mostrar Diagrama UML de Componentes");
                Console.WriteLine("10. Mostrar e Explicar Interface IPredictor");
                Console.WriteLine("0. Voltar ao menu principal");
                Console.Write("Digite o número da opção desejada: ");

                string maintenanceInput = Console.ReadLine()?.Trim() ?? string.Empty;

                if (maintenanceInput == "0")
                {
                    returnToMainMenu = true;
                }
                else
                {
                    ApplicationController.Instance.HandleMaintenanceOption(maintenanceInput).Wait();
                }
            } while (!returnToMainMenu);

            ShowMenu();
        }

        // Abre uma imagem usando o visualizador padrão do sistema
        public static void OpenImage(string fileName)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = fileName,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao abrir a imagem: {ex.Message}");
            }
        }

        // Lista todos os arquivos do projeto
        public static void ListProjectFiles()
        {
            Console.WriteLine("\n--- Localização dos Ficheiros ---");
            string[] directories = { ".", "./bin", "./obj", "./realEstateModel", "./reports" };
            foreach (var dir in directories)
            {
                Console.WriteLine($"\nDiretório: {dir}");
                string[] files = Directory.GetFiles(dir);
                foreach (var file in files)
                {
                    Console.WriteLine(file);
                }
            }
            Console.WriteLine("--- Fim da Localização dos Ficheiros ---\n");
        }

        // Exibe os componentes do programa
        public static void ShowComponents()
        {
            Console.WriteLine("\n--- Componentes do Programa ---");
            Console.WriteLine("1. SharpCEstate: Ponto de entrada do utilizador.");
            Console.WriteLine("   - Main: Inicia a aplicação.");
            Console.WriteLine("2. UserInteractionView: Gerencia a interação e visualização do utilizador.");
            Console.WriteLine("   - PrepareInterface: Prepara a interface do utilizador.");
            Console.WriteLine("   - ShowForecast: Exibe a previsão de preço.");
            Console.WriteLine("   - ShowReport: Exibe o relatório gerado.");
            Console.WriteLine("   - MenuManutencao: Exibe o menu de manutenção.");
            Console.WriteLine("3. ApplicationController: Controla a lógica da aplicação, manipula dados e gerencia o fluxo de informações.");
            Console.WriteLine("   - StartApplicationAsync: Inicia a aplicação de forma assíncrona.");
            Console.WriteLine("   - HandleUserInteractions: Lida com as interações do utilizador.");
            Console.WriteLine("   - PredictPriceAsync: Prevê o preço de um imóvel de forma assíncrona.");
            Console.WriteLine("   - GeneratePriceReportAsync: Gera um relatório de preços baseado na área e tipo de imóvel.");
            Console.WriteLine("   - GenerateReportForMaxBudgetAsync: Gera um relatório de imóveis que se encaixam no orçamento máximo especificado.");
            Console.WriteLine("4. RealEstateDataProcessor: Implementa a interface IPredictor para processar dados e fazer previsões.");
            Console.WriteLine("   - LoadAndPrepareData: Carrega e prepara os dados.");
            Console.WriteLine("   - LoadModel: Carrega o modelo de previsão.");
            Console.WriteLine("   - ProcessData: Processa os dados de entrada.");
            Console.WriteLine("   - ExecuteMLNET: Executa operações do ML.NET.");
            Console.WriteLine("5. RealEstateData: Define a estrutura dos dados imobiliários.");
            Console.WriteLine("6. RealEstatePrediction: Define a estrutura das previsões imobiliárias.");
            Console.WriteLine("7. IPredictor: Interface para classes que implementam previsão de preços de imóveis.");
            Console.WriteLine("8. External Libraries: Bibliotecas externas utilizadas.");
            Console.WriteLine("   - System: Biblioteca do sistema.");
            Console.WriteLine("   - Microsoft.ML: Biblioteca ML.NET.");
            Console.WriteLine("\nFluxo de Trabalho:");
            Console.WriteLine("1. O utilizador inicia o programa através de SharpCEstate.");
            Console.WriteLine("2. ApplicationController coordena a inicialização, carregamento de dados e treinamento do modelo.");
            Console.WriteLine("3. RealEstateDataProcessor manipula dados e executa operações ML.NET.");
            Console.WriteLine("4. UserInteractionView exibe as informações e coleta entradas do utilizador.");
            Console.WriteLine("5. RealEstateData define os dados e previsões.");
            Console.WriteLine("6. RealEstatePrediction retorna as predições.");
            Console.WriteLine("7. IPredictor é usado pelo RealEstateDataProcessor para previsões.");
            Console.WriteLine("8. System e Microsoft.ML são bibliotecas externas utilizadas.");
            Console.WriteLine("--- Fim dos Componentes do Programa ---\n");
        }

        // Exibe a estrutura detalhada do programa
        public static void ShowDetailedStructure()
        {
            Console.WriteLine("\n--- Estrutura Detalhada do Programa ---");
            Console.WriteLine("SharpCEstate/");
            Console.WriteLine("│");
            Console.WriteLine("├── bin/                  # Ficheiros compilados");
            Console.WriteLine("│   └── Debug/");
            Console.WriteLine("├── obj/                  # Ficheiros de objetos compilados");
            Console.WriteLine("│   ├── Debug/");
            Console.WriteLine("│   ├── SharpCEstate.csproj.nuget.dgspec.json");
            Console.WriteLine("│   ├── SharpCEstate.csproj.nuget.g.props");
            Console.WriteLine("│   ├── SharpCEstate.csproj.nuget.g.targets");
            Console.WriteLine("│   ├── project.assets.json");
            Console.WriteLine("│   └── project.nuget.cache");
            Console.WriteLine("├── realEstateModel/      # Modelos treinados");
            Console.WriteLine("│   ├── TrainingInfo");
            Console.WriteLine("│   └── TransformerChain");
            Console.WriteLine("├── reports/              # Relatórios gerados");
            Console.WriteLine("│   ├── price_report_tx_area_YYYYMMDD.html");
            Console.WriteLine("│   └── budget_report_tx_price_YYYYMMDD.html");
            Console.WriteLine("├── Controller.cs    # Código-fonte");
            Console.WriteLine("├── Model.cs");
            Console.WriteLine("├── SharpCEstate.cs");
            Console.WriteLine("├── View.cs");
            Console.WriteLine("├── realEstateModel.zip");
            Console.WriteLine("├── data.csv");
            Console.WriteLine("└── README.md             # Documentação do projeto");
            Console.WriteLine("--- Fim da Estrutura Detalhada ---\n");
        }

        // Exibe o catálogo de erros
        public static void ShowErrorCatalog()
        {
            Console.WriteLine("\n--- Catálogo de Erros ---");
            Console.WriteLine("Erro [DataLoadingError]: Falha ao carregar os dados. Verifique se o caminho do arquivo está correto e se o arquivo está acessível.");
            Console.WriteLine("Erro [LoadModelError]: Falha ao carregar o modelo. Verifique se o caminho do arquivo está correto e se o arquivo está acessível.");
            Console.WriteLine("Erro [ModelTrainingError]: Falha ao treinar o modelo. Verifique os dados de entrada e os parâmetros do modelo.");
            Console.WriteLine("Erro [ModelSavingError]: Falha ao salvar o modelo. Verifique o caminho do arquivo e a permissão de escrita.");
            Console.WriteLine("Erro [PredictionError]: Falha ao prever o preço. Verifique os dados de entrada e o estado do modelo.");
            Console.WriteLine("Erro [ViewUpdateError]: Falha ao atualizar a interface. Verifique a conexão com a interface de usuário.");
            Console.WriteLine("Erro [InvalidUserInputError]: Entrada do usuário inválida. Verifique os dados de entrada fornecidos.");
            Console.WriteLine("Erro [UnknownError]: Erro desconhecido. Consulte os logs para mais detalhes.");
            Console.WriteLine("--- Fim do Catálogo de Erros ---\n");
        }

        // Exibe a verificação de delegados
        public static void ShowDelegates()
        {
            Console.WriteLine("\n--- Verificação de Delegados ---");
            Console.WriteLine("Tabela 1: Componentes Emissores e Recetores de Eventos");
            Console.WriteLine("------------------------------------------------------------------------------------------------------------");
            Console.WriteLine("| Emissor/Recetor    | Model Emissor (ME)        | View Emissor (VE)          | Controller Emissor (CE)    |");
            Console.WriteLine("------------------------------------------------------------------------------------------------------------");
            Console.WriteLine("| Model Recetor      | -                         | (VE) PredictPriceAsync     | (CE) StartApplicationAsync |");
            Console.WriteLine("| View Recetor       | (ME) ShowReport           | -                          | (CE) ShowForecast          |");
            Console.WriteLine("| Controller Recetor | (ME) ModelAndDataLoaded   | -                          | -                          |");
            Console.WriteLine("------------------------------------------------------------------------------------------------------------");
            Console.WriteLine("\nDelegados verificados com sucesso.");
            Console.WriteLine("--- Fim da Verificação de Delegados ---\n");
        }

        // Explica a interface IPredictor
        public static void ExplainIPredictorInterface()
        {
            Console.WriteLine("\n--- Interface IPredictor ---");
            Console.WriteLine("A interface IPredictor define o contrato para classes que implementam previsão de preços de imóveis.");
            Console.WriteLine("Métodos:");
            Console.WriteLine("1. Task<float> PredictPriceAsync(RealEstateData data): Método assíncrono que prevê o preço com base nos dados fornecidos.");
            Console.WriteLine("2. void LoadModel(string modelPath): Carrega o modelo de previsão a partir do caminho especificado.");
            Console.WriteLine("3. void TrainModel(IDataView trainingData): Treina o modelo com os dados de treinamento fornecidos.");
            Console.WriteLine("4. void SaveModel(string modelPath): Salva o modelo no caminho especificado.");
            Console.WriteLine("5. Task<string> GeneratePriceReportAsync(float area, string propertyType): Gera um relatório de preços baseado na área e tipo de imóvel.");
            Console.WriteLine("6. Task<string> GenerateReportForMaxBudgetAsync(float maxBudget, string propertyType): Gera um relatório de imóveis que se encaixam no orçamento máximo especificado.");
            Console.WriteLine("--- Fim da Interface IPredictor ---\n");
        }
    }
}
