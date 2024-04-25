using System;
using Microsoft.ML;

namespace SharpCEstate
{
    class SharpCEstate
{
    static void Main(string[] args)
    {
        MLContext mlContext = new MLContext(seed: 0);
        Configurations.Load();

        var model = ModelInitializer.InitializeModel(mlContext);
        if (model == null)
        {
            var dataView = RealEstateDataProcessor.LoadAndPrepareData(mlContext, "imovirtual_casas.csv");
            DataDebuggingTools.PrintDataViewSchema(dataView);  // Chamada da função para imprimir o esquema

            var dataSplit = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
            var trainingData = dataSplit.TrainSet;
            model = RealEstateDataProcessor.TrainModel(mlContext, trainingData);
            MachineLearningModel.SaveModel(mlContext, model, trainingData.Schema);
        }

        var testData = RealEstateDataProcessor.LoadAndPrepareData(mlContext, "imovirtual_casas.csv");
        DataDebuggingTools.PrintDataViewSchema(testData);  // Repetir antes da execução para verificação
        MachineLearningModel.ExecuteMLNET(mlContext, model, testData);
        
        Console.WriteLine("A aplicação começou. Pressione qualquer tecla para sair.");
        Console.ReadKey();
    }
}
}
