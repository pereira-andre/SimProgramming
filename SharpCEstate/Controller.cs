/*
** ficheiro: Controller.cs
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
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.ML;

namespace SharpCEstate
{
    public class ApplicationController
    {
        // Delegate para tratamento de erros
        public delegate void ErrorHandler(ErrorCode code, string message);
        public event ErrorHandler? ErrorOccurred;

        // Singleton instance do controlador da aplicação
        private static ApplicationController? instance;
        // Contexto ML.NET
        private MLContext mlContext = new MLContext(seed: 0);
        // Interface preditor
        private IPredictor predictor;

        // Caminhos para o modelo e dados
        private string modelPath = "./realEstateModel.zip";
        private string dataPath = "./data.csv";

        // Delegates
        public delegate void StartApplicationAsyncHandler();
        public delegate void ShowForecastHandler(float predictedPrice);

        // Eventos
        public event StartApplicationAsyncHandler? StartApplicationAsyncCompleted;
        public event ShowForecastHandler? ShowForecastCompleted;

        // Construtor privado para o padrão Singleton
        private ApplicationController()
        {
            ErrorOccurred += HandleError;
            predictor = new RealEstateDataProcessor(mlContext, modelPath);
            ((RealEstateDataProcessor)predictor).ModelAndDataLoaded += OnModelAndDataLoaded;
            ((RealEstateDataProcessor)predictor).PredictPriceAsyncCompleted += OnPredictPriceAsyncCompleted;
        }

        // Propriedade para acessar a instância Singleton
        public static ApplicationController Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ApplicationController();
                }
                return instance;
            }
        }

        // Método para iniciar a aplicação de forma assíncrona
        public async Task StartApplicationAsync()
        {
            try
            {
                var dataView = RealEstateDataProcessor.LoadAndPrepareData(mlContext, dataPath);
                var splitData = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
                var trainingData = splitData.TrainSet;
                predictor.TrainModel(trainingData);
                predictor.SaveModel(modelPath);
                UserInteractionView.PrepareInterface();
                OnStartApplicationAsyncCompleted(); // Evento StartApplicationAsyncCompleted
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(ErrorCode.UnknownError, $"Erro durante a inicialização da aplicação: {ex.Message}");
            }
            await Task.CompletedTask;
        }

        // Método para executar a checklist de verificação
        public void RunCheckList()
        {
            Console.WriteLine("\n--- Checklist de Verificação ---");

            bool dataLoadSuccess = VerifyDataLoading();
            bool modelLoadSuccess = VerifyModelLoading();
            bool modelTrainSuccess = VerifyModelTraining();
            bool modelSaveSuccess = VerifyModelSaving();
            bool predictionSuccess = VerifyPrediction();
            bool viewUpdateSuccess = VerifyViewUpdate();
            bool modelComponentSuccess = VerifyModelComponents();
            bool viewInformationPassSuccess = VerifyViewInformationPass();

            if (dataLoadSuccess && modelLoadSuccess && modelTrainSuccess && modelSaveSuccess && predictionSuccess && viewUpdateSuccess && modelComponentSuccess && viewInformationPassSuccess)
            {
                Console.WriteLine("\nTodas as verificações foram bem sucedidas.");
            }
            else
            {
                Console.WriteLine("\nAlgumas verificações falharam. Consulte os detalhes acima.");
            }

            Console.WriteLine("--- Fim da Checklist ---\n");
        }

        // Verifica o carregamento dos dados
        private bool VerifyDataLoading()
        {
            try
            {
                var dataView = RealEstateDataProcessor.LoadAndPrepareData(mlContext, dataPath);
                bool success = dataView != null;
                Console.WriteLine("Carregamento dos dados: " + (success ? "Sucesso" : "Falha"));
                return success;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(ErrorCode.DataLoadingError, $"Carregamento dos dados: Falha - {ex.Message}");
                return false;
            }
        }

        // Verifica o carregamento do modelo
        private bool VerifyModelLoading()
        {
            try
            {
                predictor.LoadModel(modelPath);
                Console.WriteLine("Carregamento do modelo: Sucesso");
                return true;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(ErrorCode.LoadModelError, $"Carregamento do modelo: Falha - {ex.Message}");
                return false;
            }
        }

        // Verifica o treino do modelo
        private bool VerifyModelTraining()
        {
            try
            {
                var dataView = RealEstateDataProcessor.LoadAndPrepareData(mlContext, dataPath);
                var splitData = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
                var trainingData = splitData.TrainSet;
                predictor.TrainModel(trainingData);
                Console.WriteLine("Treino do modelo: Sucesso");
                return true;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(ErrorCode.ModelTrainingError, $"Treinamento do modelo: Falha - {ex.Message}");
                return false;
            }
        }

        // Verifica a gravação do modelo
        private bool VerifyModelSaving()
        {
            try
            {
                predictor.SaveModel(modelPath);
                Console.WriteLine("Gravação do modelo: Sucesso");
                return true;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(ErrorCode.ModelSavingError, $"Gravação do modelo: Falha - {ex.Message}");
                return false;
            }
        }

        // Verifica a previsão de preços
        private bool VerifyPrediction()
        {
            try
            {
                var data = new RealEstateData { Area = 100, Localizacao = "Lisboa", Nome = "t2" };
                var predictedPrice = predictor.PredictPriceAsync(data).Result;
                bool success = predictedPrice > 0;
                Console.WriteLine("Previsão de preço: " + (success ? "Sucesso" : "Falha"));
                return success;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(ErrorCode.PredictionError, $"Previsão de preço: Falha - {ex.Message}");
                return false;
            }
        }

        // Verifica a atualização da interface
        private bool VerifyViewUpdate()
        {
            try
            {
                UserInteractionView.PrepareInterface();
                UserInteractionView.ShowForecast(250000);
                Console.WriteLine("Atualização da interface: Sucesso");
                return true;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(ErrorCode.ViewUpdateError, $"Atualização da interface: Falha - {ex.Message}");
                return false;
            }
        }

        // Verifica os componentes do modelo
        private bool VerifyModelComponents()
        {
            try
            {
                var data = new RealEstateData { Area = 100, Localizacao = "Lisboa", Nome = "t2" };
                var prediction = new RealEstatePrediction { PredictedPrice = 250000 };

                bool success = data != null && prediction != null;
                Console.WriteLine("Componentes do modelo: " + (success ? "Sucesso" : "Falha"));
                return success;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(ErrorCode.UnknownError, $"Componentes do modelo: Falha - {ex.Message}");
                return false;
            }
        }

        // Verifica a passagem de informação para a View
        private bool VerifyViewInformationPass()
        {
            try
            {
                UserInteractionView.ShowMenuTitle();
                Console.WriteLine("Passagem de informação à View: Sucesso");
                return true;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(ErrorCode.ViewUpdateError, $"Passagem de informação à View: Falha - {ex.Message}");
                return false;
            }
        }

        // Lida com os erros
        private void HandleError(ErrorCode code, string message)
        {
            Console.WriteLine($"Erro [{code}]: {message}");
            if (code == ErrorCode.InvalidUserInputError)
            {
                Console.WriteLine("Por favor, tente novamente.");
            }
            else
            {
                Console.WriteLine("Deseja tentar corrigir o erro? (Y/N)");
                string? response = Console.ReadLine()?.Trim().ToUpper();
                if (response == "Y")
                {
                    Console.WriteLine("Reiniciando a operação...");
                    UserInteractionView.ShowMenu();
                }
                else
                {
                    Console.WriteLine("Operação cancelada pelo usuário.");
                }
            }
        }

        // Lida com a solicitação do usuário de forma assíncrona
        public async Task HandleUserRequestAsync(string[] inputs)
        {
            if (inputs.Length >= 3)
            {
                try
                {
                    float area = float.Parse(inputs[0].Trim());
                    string location = inputs[1].Trim();
                    string propertyType = inputs[2].Trim();

                    if (area <= 0)
                    {
                        ErrorOccurred?.Invoke(ErrorCode.InvalidUserInputError, "A área deve ser maior do que zero.");
                        return;
                    }

                    if (!System.Text.RegularExpressions.Regex.IsMatch(propertyType, "^t[0-9]$") && !System.Text.RegularExpressions.Regex.IsMatch(propertyType, "^t10$"))
                    {
                        ErrorOccurred?.Invoke(ErrorCode.InvalidUserInputError, "O tipo de imóvel deve ser entre t0 e t10.");
                        return;
                    }

                    var predictedPrice = await predictor.PredictPriceAsync(new RealEstateData { Area = area, Localizacao = location, Nome = propertyType });
                    UserInteractionView.ShowForecast(predictedPrice);
                    OnShowForecastCompleted(predictedPrice); // Evento ShowForecastCompleted
                }
                catch (FormatException)
                {
                    ErrorOccurred?.Invoke(ErrorCode.InvalidUserInputError, "Formato de entrada inválido para o número.");
                }
                catch (Exception ex)
                {
                    ErrorOccurred?.Invoke(ErrorCode.PredictionError, $"Erro ao prever o preço: {ex.Message}");
                }
            }
            else
            {
                ErrorOccurred?.Invoke(ErrorCode.InvalidUserInputError, "Número insuficiente de entradas. Por favor, forneça área, localização e tipo.");
            }
        }

        // Gera relatório de preço de forma assíncrona
        public async Task<string> GeneratePriceReportAsync(float area, string propertyType)
        {
            return await predictor.GeneratePriceReportAsync(area, propertyType);
        }

        // Gera relatório com orçamento máximo de forma assíncrona
        public async Task<string> GenerateReportForMaxBudgetAsync(float maxBudget, string propertyType)
        {
            return await predictor.GenerateReportForMaxBudgetAsync(maxBudget, propertyType);
        }

        // Lista os arquivos do projeto
        public void ListProjectFiles()
        {
            UserInteractionView.ListProjectFiles();
        }

        // Mostra os componentes do programa
        public void ShowComponents()
        {
            UserInteractionView.ShowComponents();
        }

        // Mostra a estrutura detalhada do programa
        public void ShowDetailedStructure()
        {
            UserInteractionView.ShowDetailedStructure();
        }

        // Mostra o catálogo de erros
        public void ShowErrorCatalog()
        {
            UserInteractionView.ShowErrorCatalog();
        }

        // Mostra a verificação de delegados
        public void ShowDelegates()
        {
            UserInteractionView.ShowDelegates();
        }

        // Explica a interface IPredictor
        public void ExplainIPredictorInterface()
        {
            UserInteractionView.ExplainIPredictorInterface();
        }

        // Lida com as interações do usuário
        public async Task HandleUserInteractions(string userInput)
        {
            switch (userInput)
            {
                case "1":
                    await HandleSinglePrediction();
                    break;
                case "2":
                    await HandleReportGeneration();
                    break;
                case "3":
                    await HandleMaxBudgetReportGeneration();
                    break;
                case "4":
                    UserInteractionView.ShowMaintenanceMenu();
                    string maintenanceInput = Console.ReadLine()?.Trim() ?? string.Empty;
                    await HandleMaintenanceOption(maintenanceInput);
                    break;
                case "0":
                    Console.WriteLine("Encerrando a aplicação...");
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Opção inválida. Por favor, escolha entre as opções disponíveis.");
                    break;
            }
        }

        // Lida com uma previsão única
        private async Task HandleSinglePrediction()
        {
            try
            {
                Console.WriteLine("Por favor, insira a área do imóvel (em metros quadrados):");
                float area = GetValidFloatInput();

                Console.WriteLine("Por favor, insira a localização do imóvel (ou digite 'L' para listar os distritos disponíveis):");
                string location = GetValidDistrictInput();

                Console.WriteLine("Por favor, insira o tipo do imóvel (ex: t2, t3):");
                string propertyType = GetValidPropertyTypeInput();

                await HandleUserRequestAsync(new string[] { area.ToString(), location, propertyType });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
        }

        // Gera um relatório de preços
        private async Task HandleReportGeneration()
        {
            try
            {
                Console.WriteLine("Por favor, insira a área do imóvel (em metros quadrados):");
                float area = GetValidFloatInput();

                Console.WriteLine("Por favor, insira o tipo do imóvel (ex: t2, t3):");
                string propertyType = GetValidPropertyTypeInput();

                string reportPath = await GeneratePriceReportAsync(area, propertyType);
                UserInteractionView.ShowReport(reportPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
        }

        // Gera um relatório com orçamento máximo
        private async Task HandleMaxBudgetReportGeneration()
        {
            try
            {
                Console.WriteLine("Por favor, insira o valor máximo que pode pagar (em euros):");
                float maxBudget = GetValidFloatInput();

                Console.WriteLine("Por favor, insira o tipo do imóvel (ex: t2, t3):");
                string propertyType = GetValidPropertyTypeInput();

                string reportPath = await GenerateReportForMaxBudgetAsync(maxBudget, propertyType);
                UserInteractionView.ShowReport(reportPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
        }

        // Obtém um valor float válido
        private float GetValidFloatInput()
        {
            while (true)
            {
                string input = Console.ReadLine()?.Trim() ?? string.Empty;
                if (float.TryParse(input, out float result) && result > 0)
                {
                    return result;
                }
                else
                {
                    Console.WriteLine("Entrada inválida. Por favor, insira um número válido maior que zero:");
                }
            }
        }

        // Obtém um distrito válido
        private string GetValidDistrictInput()
        {
            string[] validDistricts = new string[]
            {
                "Aveiro", "Beja", "Braga", "Bragança", "Castelo Branco", "Coimbra",
                "Évora", "Faro", "Guarda", "Leiria", "Lisboa", "Portalegre",
                "Porto", "Santarém", "Setúbal", "Viana do Castelo", "Vila Real", "Viseu"
            };

            while (true)
            {
                string input = Console.ReadLine()?.Trim() ?? string.Empty;
                if (input.Equals("L", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Distritos disponíveis:");
                    foreach (var district in validDistricts)
                    {
                        Console.WriteLine($"- {district}");
                    }
                }
                else if (Array.Exists(validDistricts, district => district.Equals(input, StringComparison.OrdinalIgnoreCase)))
                {
                    return input;
                }
                else
                {
                    Console.WriteLine("Entrada inválida. Por favor, insira um distrito válido da lista (ou digite 'L' para listar os distritos disponíveis):");
                }
            }
        }

        // Obtém um tipo de propriedade válido
        private string GetValidPropertyTypeInput()
        {
            while (true)
            {
                string input = Console.ReadLine()?.Trim() ?? string.Empty;
                if (System.Text.RegularExpressions.Regex.IsMatch(input, "^t[0-9]$") || System.Text.RegularExpressions.Regex.IsMatch(input, "^t10$"))
                {
                    return input;
                }
                else
                {
                    Console.WriteLine("Entrada inválida. Por favor, insira um tipo de imóvel válido (ex: t2, t3):");
                }
            }
        }

        // Lida com a opção de manutenção
        public async Task HandleMaintenanceOption(string maintenanceInput)
        {
            switch (maintenanceInput)
            {
                case "1":
                    Console.WriteLine("Deseja realmente executar a checklist de verificação? (Y/N)");
                    string? confirmation = Console.ReadLine()?.Trim().ToUpper();
                    if (confirmation == "Y")
                    {
                        RunCheckList();
                    }
                    break;
                case "2":
                    ShowErrorCatalog();
                    break;
                case "3":
                    ListProjectFiles();
                    break;
                case "4":
                    ShowComponents();
                    break;
                case "5":
                    ShowDetailedStructure();
                    break;
                case "6":
                    ShowDelegates();
                    break;
                case "7":
                    OpenImage("uml/ProgramUML.png");
                    break;
                case "8":
                    OpenImage("uml/ErrorCaptureUML.png");
                    break;
                case "9":
                    OpenImage("uml/ComponentDiagram.png");
                    break;
                case "10":
                    ExplainIPredictorInterface();
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("Opção inválida. Por favor, escolha entre as opções disponíveis.");
                    break;
            }
        }

        // Abre uma imagem usando o visualizador padrão do sistema
        private void OpenImage(string fileName)
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

        // Evento ModelAndDataLoaded
        protected virtual void OnModelAndDataLoaded()
        {
            ModelAndDataLoaded?.Invoke(this, EventArgs.Empty);
        }

        // Evento PredictPriceAsyncCompleted
        protected virtual void OnPredictPriceAsyncCompleted(float predictedPrice)
        {
            PredictPriceAsyncCompleted?.Invoke(this, predictedPrice);
        }

        // Evento StartApplicationAsyncCompleted
        protected virtual void OnStartApplicationAsyncCompleted()
        {
            StartApplicationAsyncCompleted?.Invoke();
        }

        // Evento ShowForecastCompleted
        protected virtual void OnShowForecastCompleted(float predictedPrice)
        {
            ShowForecastCompleted?.Invoke(predictedPrice);
        }

        // Eventos
        public event EventHandler? ModelAndDataLoaded;
        public event EventHandler<float>? PredictPriceAsyncCompleted;
    }
}
