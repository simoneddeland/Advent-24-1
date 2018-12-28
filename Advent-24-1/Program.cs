using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advent_24_1
{
    class Program
    {
        static void Main(string[] args)
        {
            var fileLines = File.ReadAllLines("input24.txt");
            var boost = 29;

            // DEBUG
            //fileLines = File.ReadAllLines("input24test.txt");

            var immuneSystem = new List<Group>();
            var infection = new List<Group>();

            // PARSE INPUT
            bool readingImmunesystem = true;
            int groupNumber = 1;

            for (int i = 1; i < fileLines.Length; i++)
            {
                var line = fileLines[i];
                if (line == "")
                {
                    continue;
                }
                else if (line == "Infection:")
                {
                    readingImmunesystem = false;
                    groupNumber = 1;
                    continue;
                }

                var newGroup = new Group();

                var splitLine = line.Split(new string[] { " units", "each with ", " hit points ", "with an attack that does ", " damage at initiative " }, StringSplitOptions.None);
                splitLine[3] = splitLine[3].Trim();
                newGroup.Size = int.Parse(splitLine[0]);
                newGroup.HP = int.Parse(splitLine[2]);

                var attackStringSplit = splitLine[4].Split(' ');
                newGroup.AttackDamage = int.Parse(attackStringSplit[0]);
                newGroup.AttackType = attackStringSplit[1];
                newGroup.Initiative = int.Parse(splitLine[5]);

                // PARSE WEAKNESSES AND IMMUNITIES
                if (splitLine[3] != "")
                {
                    var weakImm = splitLine[3].Trim('(', ')');
                    var wiList = weakImm.Split(new string[] { "; " }, StringSplitOptions.None);

                    foreach (var weakOrImm in wiList)
                    {
                        var splitWeakOrImm = weakOrImm.Split(new string[] { " to " }, StringSplitOptions.None);
                        var listToAdd = splitWeakOrImm[0] == "weak" ? newGroup.Weaknesses : newGroup.Immunities;

                        var weakOrImParams = splitWeakOrImm[1].Split(new string[] { ", "}, StringSplitOptions.None);
                        foreach (var item in weakOrImParams)
                        {
                            listToAdd.Add(item);
                        }
                    }
                }

                if (readingImmunesystem)
                {
                    newGroup.Name = "Immune system group " + groupNumber;
                    newGroup.AttackDamage += boost;
                    immuneSystem.Add(newGroup);
                }
                else
                {
                    newGroup.Name = "Infection group " + groupNumber;
                    infection.Add(newGroup);
                }
                groupNumber++;

                Console.WriteLine(newGroup.ToString());

            }

            Console.WriteLine("Parsing complete");
            Console.WriteLine();

            int roundNumber = 1;

            // BATTLE
            while (immuneSystem.Count > 0 && infection.Count > 0)
            {
                var prevNumberOfUnits = immuneSystem.Sum(x => x.Size) + infection.Sum(x => x.Size);
                /*
                Console.WriteLine($"Round {roundNumber}");
                Console.WriteLine($"Immune system:");
                if (immuneSystem.Count == 0)
                {
                    Console.WriteLine("No groups remain");
                }
                else
                {
                    foreach (var group in immuneSystem)
                    {
                        Console.WriteLine($"{group.Name} contains {group.Size} units");
                    }
                }
                Console.WriteLine($"Infection:");
                if (infection.Count == 0)
                {
                    Console.WriteLine("No groups remain");
                }
                else
                {
                    foreach (var group in infection)
                    {
                        Console.WriteLine($"{group.Name} contains {group.Size} units");
                    }
                }
                */
 
                var alreadyTargeted = new List<Group>();

                void AcquireTargets(Group item)
                {
                    item.Target = null;
                    List<Group> defendingArmy = immuneSystem;
                    if (immuneSystem.Contains(item))
                    {
                        defendingArmy = infection;
                    }

                    var bestDamage = int.MinValue;
                    Group target = null;
                    

                    foreach (var possibleTarget in defendingArmy)
                    {
                        if (alreadyTargeted.Contains(possibleTarget))
                        {
                            continue;
                        }

                        var currentDamage = possibleTarget.DamageTaken(item.EffectivePower(), item.AttackType);
                        if (currentDamage == 0)
                        {
                            continue;
                        }

                        if (currentDamage > bestDamage)
                        {
                            bestDamage = currentDamage;
                            target = possibleTarget;
                        }
                        else if (currentDamage == bestDamage)
                        {
                            if (possibleTarget.EffectivePower() > target.EffectivePower())
                            {
                                target = possibleTarget;
                            }
                            else if (possibleTarget.EffectivePower() == target.EffectivePower())
                            {
                                if (possibleTarget.Initiative > target.Initiative)
                                {
                                    target = possibleTarget;
                                }
                            }
                        }

                    }
                    item.Target = target;
                    alreadyTargeted.Add(target);

                }

                var allRemainingGroups = new List<Group>();
                allRemainingGroups.AddRange(immuneSystem);
                allRemainingGroups.AddRange(infection);

                allRemainingGroups.Sort((x, y) => x.EffectivePower() > y.EffectivePower() ? -1 : x.EffectivePower() < y.EffectivePower() ? 1 : x.Initiative > y.Initiative ? -1 : 1);
                foreach (var item in allRemainingGroups)
                {
                    AcquireTargets(item);
                }


                allRemainingGroups.Sort((x, y) => x.Initiative > y.Initiative ? -1 : 1);
                // DAMAGE PHASE
                foreach (var item in allRemainingGroups)
                {
                    if (item.Target != null)
                    {
                        item.Target.TakeDamage(item.EffectivePower(), item.AttackType);
                        //Console.WriteLine($"{item.Target.DamageTaken(item.EffectivePower(), item.AttackType)} damage will be taken");
                    }
                    
                }

                // REMOVE ALL DEAD GROUPS
                //Console.WriteLine("Removing dead groups");
                for (int i = immuneSystem.Count - 1; i >= 0; i--)
                {
                    if (immuneSystem[i].Size == 0)
                    {
                        immuneSystem.RemoveAt(i);
                    }
                }

                for (int i = infection.Count - 1; i >= 0; i--)
                {
                    if (infection[i].Size == 0)
                    {
                        infection.RemoveAt(i);
                    }
                }
                //Console.WriteLine($"Round {roundNumber} over");
                roundNumber++;
                //Console.WriteLine();

                if (prevNumberOfUnits == immuneSystem.Sum(x => x.Size) + infection.Sum(x => x.Size))
                {
                    Console.WriteLine("No units killed this round, tie detected");
                    break;
                }

            }
            Console.WriteLine("Battle over");
            Console.WriteLine($"Infection: {infection.Sum(x => x.Size)} units left");
            Console.WriteLine($"Immune system: {immuneSystem.Sum(x => x.Size)} units left");
            Console.ReadKey();
        }
    }
}
