using Accord.Controls;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Math.Optimization.Losses;
using Accord.Statistics;
using Accord.Statistics.Kernels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Main();
    }

    // Update is called once per frame
    void Update()
    {

    }

    [MTAThread]
    static void Main()
    {
        double[][] inputs =
        {
                /* 1.*/ new double[] { 0, 0 },
                /* 2.*/ new double[] { 1, 0 }, 
                /* 3.*/ new double[] { 0, 1 }, 
                /* 4.*/ new double[] { 1, 1 },
            };

        int[] outputs =
        { 
                /* 1. 0 xor 0 = 0: */ 0,
                /* 2. 1 xor 0 = 1: */ 1,
                /* 3. 0 xor 1 = 1: */ 1,
                /* 4. 1 xor 1 = 0: */ 0,
            };

        // Create the learning algorithm with the chosen kernel
        var smo = new SequentialMinimalOptimization<Gaussian>()
        {
            Complexity = 100 // Create a hard-margin SVM 
        };

        // Use the algorithm to learn the svm
        var svm = smo.Learn(inputs, outputs);

        // Compute the machine's answers for the given inputs
        bool[] prediction = svm.Decide(inputs);

        // Compute the classification error between the expected 
        // values and the values actually predicted by the machine:
        double error = new AccuracyLoss(outputs).Loss(prediction);

        print("Error: " + error);
        var result = prediction;
        foreach (var value in result) {
            print(value);

        }
        // Show results on screen 
/*        ScatterplotBox scatterplotBox = ScatterplotBox.Show("Training data", inputs, outputs);
        ScatterplotBox scatterplotBox2 = ScatterplotBox.Show("SVM results", inputs, prediction.ToZeroOne());*/

        /*Console.ReadKey();*/
    }
}
