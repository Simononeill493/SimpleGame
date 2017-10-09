﻿
using SimpleGame.AI.GeneticAlgorithm;
using SimpleGame.DataPayloads.DiscreteData;
using SimpleGame.Deciders;
using SimpleGame.Deciders.DecisionMatrix;
using SimpleGame.Games;
using SimpleGame.Games.FoodEatingGame;
using SimpleGame.Games.SimplePacman;
using System;
using System.Collections.Generic;
using log4net;
using Pacman;
using SimpleGame.Metrics;

namespace SimpleGame
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = SimpleGameLoggerManager.SetupLogger();
            logger.Debug("Simple Game Logger Created");

            /*var genAlg = new GeneticAlgorithmRunner
            (
                numGenerations: 20000,
                numToKill: 13,
                numInGeneration: 20,
                numOfTimesToTestASpecies: 1,
                mutationRate: 0.2,
                deciderType: DeciderType.LazyMatrix
            );*/


            var runner = new PacmanManager();
            var stateProvider = (PacmanStateProvider)runner.StateProvider;
            var tester = new SimpleGameTester();

            tester.GeneticAlgorithmTests(runner,stateProvider);

            //runner.Demonstrate(new SimpleGame.Deciders.Discrete.RandomDiscreteDecider(new Random(), runner.IOInfo), stateProvider.GetStateForDemonstration());
            //Console.ReadLine();
            //return;

            //var decider = genAlg.Train(runner, stateProvider, showProgress: true, demonstrateEveryXIterations: 250);
            //ActualPacmanGameInstance.isLogging = true;

            //var state = stateProvider.GetStateForDemonstration();
            //runner.Demonstrate(decider, state);
            //state.Dispose();


            Console.WriteLine("Finished.");
            Console.ReadLine();
        }
    }
}
