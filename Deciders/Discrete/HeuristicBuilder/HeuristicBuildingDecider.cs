﻿using SimpleGame.Deciders.Discrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleGame.AI.GeneticAlgorithm;
using SimpleGame.DataPayloads.DiscreteData;
using SimpleGame.Deciders.HeuristicBuilder;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using SimpleGame.Deciders.Discrete.HeuristicBuilder;
using SimpleGame.AI;

namespace SimpleGame.Deciders
{
    [Serializable()]
    public class HeuristicBuildingDecider : IDiscreteDecider
    {
        public static int maxGenes = 100;

        public DiscreteIOInfo IOInfo { get; }
        public int NumGenes => Heuristics.Count;

        public List<Heuristic> Heuristics;

        public Random R;

        public HeuristicBuildingDecider(Random r,DiscreteIOInfo ioInfo)
        {
            Heuristics = new List<Heuristic>();
            IOInfo = ioInfo;
            R = r;
        }

        public IDiscreteDecider CrossMutate(IDiscreteDecider decider2, double mutationRate, Random r)
        {
            return HeuristicDeciderFactory.CrossMutate(this, (HeuristicBuildingDecider)decider2, mutationRate, r);
        }

        public IDiscreteDecider GetMutated(double mutationRate,Random r)
        {
            return HeuristicDeciderFactory.GetMutated(this, mutationRate, r);
        }

        public DiscreteDataPayload Decide(DiscreteDataPayload input)
        {
            var h = GetHeuristicFromListFor(input);

            if(h==null)
            {
                //h = Heuristic.CreateHeuristicRandomlyFromThisInput(_r, IOInfo, input, HeuristicBuildingConstants.ConditionsToAddToHeuristicFromInput);
                h = GetExactHeuristicOrRandomForThisInput(input);
                Heuristics.Add(h);
            }
            DiscreteDataPayload decision = new DiscreteDataPayload(IOInfo.OutputInfo.PayloadType, h.ExpectedOutput);

            return decision;
        }

        public Heuristic GetExactHeuristicOrRandomForThisInput(DiscreteDataPayload input)
        {
            Heuristic decidedHeuristic = GetHeuristicFromListFor(input);
            if (decidedHeuristic != null)
            {
                return decidedHeuristic;
            }
            else
            {
                Heuristic h = HeuristicFactory.CreateExactHeuristicFromThisInput(R, IOInfo, input);
                //Heuristic heuristic = Heuristic.CreateHeuristicRandomlyFromThisInput(_r, IOInfo, input, HeuristicBuildingConstants.ConditionsToAddToHeuristicFromInput);

                Heuristics.Add(h);
                return h;
            }
        }

        public Heuristic GetHeuristicFromListFor(DiscreteDataPayload input)
        {
            Heuristic heuristicToUse = null;

            foreach(var h in Heuristics)
            {
                foreach(var con in h.Conditions)
                {
                    if(input.Data[con.Item1] != con.Item2)
                    {
                        goto CheckNext;
                    }
                }

                foreach (var ex in h.Exceptions)
                {
                    if (input.Data[ex.Item1] == ex.Item2)
                    {
                        goto CheckNext;
                    }
                }

                heuristicToUse = h;
                break;

                CheckNext: continue;
            }

            if(heuristicToUse != null)
            {
                heuristicToUse.UseCount++;
                return heuristicToUse;
            }

            return null;
        }

        public void AddRandomHeuristics(int numToAdd)
        {
            for(int i=0;i<numToAdd;i++)
            {
                var newHeuristic = HeuristicFactory.CreateRandom(R, IOInfo,
                    HeuristicBuildingConstants.ConditionsToAddToRandomHeuristic,
                    HeuristicBuildingConstants.ExceptionsToAddToRandomHeuristic);

                var position = R.Next(0, Heuristics.Count);
                Heuristics.Insert(position, newHeuristic);
            }
        }

        public void RemoveRandomHeuristics(int numToTake)
        {
            for(int i=0;i<numToTake;i++)
            {
                var position = R.Next(0, Heuristics.Count);
                Heuristics.RemoveAt(position);
            }
        }

        public void RemoveRandomConditions(double oddsOfRemoval,Random r)
        {
            List<Heuristic> toRemove = new List<Heuristic>();

            foreach(var h in Heuristics)
            {
                if(r.NextDouble()<oddsOfRemoval)
                {
                    var numToRemove = r.Next(0, h.Conditions.Count);
                    h.Conditions.RemoveAt(numToRemove);

                    if(h.Conditions.Count == 0)
                    {
                        toRemove.Add(h);
                    }
                }
            }

            foreach(var h in toRemove)
            {
                Heuristics.Remove(h);
            }
        }

        public HeuristicBuildingDecider GetMutated(int steps)
        {
            HeuristicBuildingDecider decider = this.CloneWithAllHeuristics();

            for(int i=0;i<steps;i++)
            {
                if(R.NextDouble() < HeuristicBuildingConstants.OddsOfRemovingHeuristicWhenMutating)
                {
                    var geneNumToChange = R.Next(0, decider.Heuristics.Count);
                    decider.Heuristics.RemoveAt(geneNumToChange);
                }

                if(R.NextDouble() < HeuristicBuildingConstants.OddsOfChangingHeuristicOutputWhenMutating)
                {
                    var geneNumToChange = R.Next(0, decider.Heuristics.Count);
                    var geneToChange = decider.Heuristics.ElementAt(geneNumToChange);
                    geneToChange.Mutate(R);
                }

                if(R.NextDouble() < HeuristicBuildingConstants.OddsOfAddingNewHeuristicWhenMutating)
                {
                    decider.AddRandomHeuristics(1);
                }

                if (R.NextDouble() < HeuristicBuildingConstants.OddsOfShufflingWhenMutating)
                {
                    Shuffle(decider.Heuristics, R);
                }
            } 

            return decider;
        }

        public HeuristicBuildingDecider CloneWithAllHeuristics()
        {
            var decider = new HeuristicBuildingDecider(this.R,this.IOInfo);

            decider.Heuristics = new List<Heuristic>();

            foreach(Heuristic h in Heuristics)
            {
                var newHeuristic = new Heuristic(h.ExpectedOutput, IOInfo);
                newHeuristic.Exceptions = new List<Tuple<int, int>>(h.Exceptions);
                newHeuristic.Conditions = new List<Tuple<int, int>>(h.Conditions);

                decider.Heuristics.Add(newHeuristic);
            }

            return decider;
        }

        public void PostGenerationProcessing()
        {
            List<Heuristic> toRemove = new List<Heuristic>();
            foreach(var h in Heuristics)
            {
                if(h.UseCount==0)
                {
                    h.ConsecutiveGensNotUsed++;

                    if(h.ConsecutiveGensNotUsed >= HeuristicBuildingConstants.MaxAllowedGensWithNoHeuristicUses)
                    {
                        toRemove.Add(h);
                    }
                }
                else
                {
                    h.ConsecutiveGensNotUsed = 0;
                }
            }

            Console.WriteLine("\nRemoved " + toRemove.Count + " unused heuristics.\n");

            Heuristics = Heuristics.Except(toRemove).ToList();
        }


        public static void Shuffle(IList<Heuristic> list,Random r)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = r.Next(n + 1);
                Heuristic value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public string GetRaw()
        {
            StringBuilder sb = new StringBuilder();

            foreach(var h in Heuristics)
            {
                string data = "";
                foreach(var c in h.Conditions)
                {
                    data = data + " " + c.Item2;
                }
                data = data + '\t';

                data = data + h.ExpectedOutput.ToString();

                sb.AppendLine(data);
            }

            return sb.ToString();
        }
    }
}