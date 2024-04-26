// SharpCEstate.cs
using System;
using Microsoft.ML;

namespace SharpCEstate
{
    class SharpCEstate
    {
        static void Main(string[] args)
        {
            // Cria uma instância do contexto ML
            MLContext mlContext = new MLContext(seed: 0);

            // Cria e inicia o controlador da aplicação
            ApplicationController controller = new ApplicationController();
            controller.StartApplication();

            // Exibe mensagem inicial e espera interação do usuário
            Console.WriteLine("Bem-vindo ao SharpCEstate! Sua ferramenta de previsão de preços imobiliários.");
            UserInteractionView.Interact();
            
            // Mantém a aplicação rodando até que o usuário decida sair
            while (true)
            {
                UserInteractionView.Interact();
            }
        }
    }
}
