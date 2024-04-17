using System;

namespace SharpCEstate
{
    class SharpCEstate
    {
        static void Main(string[] args)
        {
            ApplicationController appController = new ApplicationController();
            appController.StartApplication();
            
            // O resto da lógica de interação do usuário pode ser implementada aqui
            // Tipo appController.RequestPriceForecast();

            Console.WriteLine("A aplicacao comecou. Carrega qualquer tecla para sair.");
            Console.ReadKey();
        }
    }
}
