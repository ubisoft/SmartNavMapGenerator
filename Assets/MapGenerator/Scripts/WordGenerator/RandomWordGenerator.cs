using System;
using System.Collections.Generic;
using System.Linq;

public static class RandomWordGenerator
{
    private static List<int[]> alreadyPicked = new List<int[]>();
    private static Random rng = new Random();

    public static string AdjAdjAnimal()
    {
        int index1, index2, index3;
        int[] sequence;
        int tries = 0;
        int maxTries = 10000;
        do
        {
            index1 = rng.Next(0, Adjectives.adjectives.Count - 1);
            index2 = rng.Next(0, Adjectives.adjectives.Count - 1);
            index3 = rng.Next(0, Animals.animals.Count - 1);
            sequence = new int[3]{index1, index2, index3};
            tries++;
        } while (alreadyPicked.Any(seq => seq.SequenceEqual(sequence)) && tries < maxTries);
        if (tries >= maxTries)
            throw new Exception($"Map name collision after {tries} tries to generate a unique one. Probably means that we need a better way to generate unique names.");

        alreadyPicked.Add(sequence);

        string adj1 = Adjectives.adjectives[index1].ToLower();
        adj1 = char.ToUpper(adj1[0]) + adj1.Substring(1);

        string adj2 = Adjectives.adjectives[index2].ToLower();
        adj2 = char.ToUpper(adj2[0]) + adj2.Substring(1);

        string animal = Animals.animals[index3].ToLower();
        animal = char.ToUpper(animal[0]) + animal.Substring(1);

        return adj1 + adj2 + animal;
    }
}