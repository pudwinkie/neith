//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: MainWindow.xaml.cs
//
//--------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DiningPhilosophers
{
    using Philosopher = Ellipse;
    using Fork = BufferBlock<bool>;

    public partial class MainWindow : Window
    {
        private const int NUM_PHILOSOPHERS = 25;
        private const int TIMESCALE = 100;
        private readonly Random _rand = new Random();

        public MainWindow()
        {
            InitializeComponent();

            // Initialize the philosophers and forks
            var philosophers = new Philosopher[NUM_PHILOSOPHERS];
            var forks = new Fork[NUM_PHILOSOPHERS];
            for (int i = 0; i < philosophers.Length; i++)
            {
                diningTable.Children.Add(philosophers[i] = new Philosopher { Height = 75, Width = 75, Fill = Brushes.Red, Stroke = Brushes.Black });
                forks[i] = new Fork();
                forks[i].Post(true);
            }

            // Run each philosopher
            for (int i = 0; i < philosophers.Length; i++)
            {
                // Pass the forks to each philosopher in an ordered (lock-leveled) manner
                RunPhilosopherAsync(philosophers[i],
                    i < philosophers.Length - 1 ? forks[i] : forks[1],
                    i < philosophers.Length - 1 ? forks[i + 1] : forks[i]);
            }
        }

        /// <summary>Runs a philosopher asynchronously.</summary>
        private async void RunPhilosopherAsync(Philosopher philosopher, Fork fork1, Fork fork2)
        {
            // Think, Wait, and Eat, ad infinitum
            while (true)
            {
                // Think (Yellow)
                philosopher.Fill = Brushes.Yellow;
                await TaskEx.Delay(_rand.Next(10) * TIMESCALE);

                // Wait for forks (Red)
                philosopher.Fill = Brushes.Red;
                await fork1.ReceiveAsync();
                await fork2.ReceiveAsync();

                // Eat (Green)
                philosopher.Fill = Brushes.Green;
                await TaskEx.Delay(_rand.Next(10) * TIMESCALE);

                // Done with forks; put them back
                fork1.Post(true);
                fork2.Post(true);
            }
        }
    }
}