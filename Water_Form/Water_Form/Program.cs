using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace WaterTowerSimulation
{
    //ENUM for tower status
    public enum TowerStatus { Full, Empty, Filling, Descending }
    //ENUM for pump status (overheated only for electric)
    public enum PumpState { Ready, Overheated, Cooling }

    // Main form that simulates the water tower system
    public class WaterTowerForm : Form
    {
        private WaterTower tower; // Water tower
        private Pump pump; // Main pump 
        private Pump electricPump; // Electric pump 
        private List<Consumer> consumers = new List<Consumer>(); // List of consumers
        private int totalConsumption; // Total water consumption rate
        private System.Windows.Forms.Timer simulationTimer; // Timer for simulation updates
        private System.Windows.Forms.Timer clockTimer; // Timer for updating the simulation clock
        private DateTime simulationTime; // Current time in the simulation

        // UI elements
        private PictureBox pumpPicture;
        private PictureBox electricPumpPicture;
        private PictureBox house1Picture;
        private PictureBox house2Picture;
        private Label statusLabel;
        private Label timeLabel;

        private readonly Rectangle towerRect = new Rectangle(300, 100, 100, 300); // Water tower dimensions as a rectangle
        private int waterLevelHeight; // Visual hight of waterLevel 
        private bool showWaterMessage; // Flag to check if text needs to be shown
        private Font messageFont = new Font("Arial", 14, FontStyle.Bold); // Font 
        private DateTime electricPumpStartTime; // Start time of the electric pump
        private DateTime overheatStartTime; // Time when the electric pump overheated
        private PumpState electricPumpState = PumpState.Ready; // Current state of the electric pump

        public WaterTowerForm()
        {
            InitializeComponents(); // Setup UI elements for Form
            LoadImages(); // Load images 
            InitializeSimulation(); // Initialize the water tower simulation
            this.Paint += DrawTower; // Subscribe to the Paint event to draw the tower
            this.Paint += DrawWaterMessage; // Subscribe to the Paint event to draw messages
        }


        // Initialize the UI elements
        private void InitializeComponents()
        {
            this.Text = "Water Tower";
            this.Size = new Size(800, 600); 
            this.DoubleBuffered = true;

            var startButton = new Button
            {
                Text = "Start",
                Location = new Point(20, 20),
                Size = new Size(80, 30)
            };

            // Start simulation when button is clicked
            startButton.Click += (s, e) =>
            {
                simulationTimer.Start();
                clockTimer.Start();
            };

            statusLabel = new Label //Status text
            {
                Location = new Point(120, 20),
                Size = new Size(250, 30),
                Font = new Font("Arial", 10)
            };

            timeLabel = new Label //Time text
            {
                Location = new Point(200, 60),
                Size = new Size(200, 30),
                Font = new Font("Arial", 10)
            };
            //Add controls to the Form

            this.Controls.AddRange(new Control[] { startButton, statusLabel, timeLabel });
        }

        // Load images 
        private void LoadImages()
        {
            try
            {
                // Images/ is a folder with our pictures
                string imagePath = Path.Combine(Application.StartupPath, "Images");

                // Pump picture
                pumpPicture = new PictureBox
                {
                    Size = new Size(100, 100),
                    Location = new Point(50, 400),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = Image.FromFile(Path.Combine(imagePath, "pump_off.png"))
                };

                // Electric pump picture
                electricPumpPicture = new PictureBox
                {
                    Size = new Size(100, 100),
                    Location = new Point(180, 400),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = Image.FromFile(Path.Combine(imagePath, "electric_pump_off.png"))
                };

                // Houses pictures
                house1Picture = new PictureBox
                {
                    Size = new Size(150, 150),
                    Location = new Point(500, 350),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = Image.FromFile(Path.Combine(imagePath, "house.png"))
                };

                house2Picture = new PictureBox
                {
                    Size = new Size(150, 150),
                    Location = new Point(650, 350),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = Image.FromFile(Path.Combine(imagePath, "house.png"))
                };
                // Adding controls to the Form
                this.Controls.AddRange(new Control[] { pumpPicture, electricPumpPicture, house1Picture, house2Picture });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Environment.Exit(1);
            }
        }

        // Initialize the simulation 
        private void InitializeSimulation()
        {
            tower = new WaterTower(1000);
            pump = new Pump(80);
            electricPump = new Pump(80);
            //Add consumers
            consumers.Add(new Consumer(25));
            consumers.Add(new Consumer(20));
            //Calculate totalConsumption of all consumers
            totalConsumption = consumers.Sum(c => c.ConsumptionRate);
            simulationTime = new DateTime(2025, 1, 1, 0, 0, 0);
            showWaterMessage = false;

            simulationTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            simulationTimer.Tick += SimulationUpdate;

            clockTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            clockTimer.Tick += (s, e) => {
                simulationTime = simulationTime.AddMinutes(10); //Adding 10 min per tick
                timeLabel.Text = $"Time: {simulationTime:HH:mm}";
            };
        }

        // Draw the water tower on the form
        private void DrawTower(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(Pens.Black, towerRect);
            var waterRect = new Rectangle(
                towerRect.X + 1,
                towerRect.Y + (towerRect.Height - waterLevelHeight),
                towerRect.Width - 2,
                waterLevelHeight
            );
            e.Graphics.FillRectangle(Brushes.DodgerBlue, waterRect);
        }
        //Draw water message if our water tower is filling
        private void DrawWaterMessage(object sender, PaintEventArgs e)
        {
            if (showWaterMessage)
            {
                var text = "Where is my water?";
                var shadowBrush = new SolidBrush(Color.FromArgb(100, Color.Black));
                var textSize = e.Graphics.MeasureString(text, messageFont);
                var x = (house1Picture.Left + house2Picture.Right) / 2 - textSize.Width / 2;
                var y = house1Picture.Top - 50;

                e.Graphics.DrawString(text, messageFont, shadowBrush, x + 2, y + 2);
                e.Graphics.DrawString(text, messageFont, Brushes.Red, x, y);
            }
        }

        //Update simulation
        private void SimulationUpdate(object sender, EventArgs e)
        {
            //Calculate Flow of water
            int totalFlow = (pump.IsActive ? pump.FlowRate : 0) + (electricPump.IsActive ? electricPump.FlowRate : 0);
            int netFlow = totalFlow - totalConsumption;

            if (netFlow > 0)
                tower.AddWater((netFlow + totalConsumption) / 6); //Divide by 6, because we put value per hour in our objects
            else
                tower.RemoveWater(-netFlow / 6);

            // Start pump
            if (tower.WaterLevel <= 0 && !pump.IsActive)
            {
                pump.Toggle();
                electricPump.Toggle();
                showWaterMessage = true;
                UpdatePumpImage();

                if (electricPump.IsActive)
                    electricPumpStartTime = simulationTime;  
            }
            
            // Stop pump
            if (tower.WaterLevel >= tower.MaxCapacity && pump.IsActive)
            {
                pump.Toggle();
                electricPump.Toggle();
                showWaterMessage = false;
                UpdatePumpImage();
            }

            // Overheating
            if (electricPump.IsActive && electricPumpState == PumpState.Ready)
            {
                var activeTime = simulationTime - electricPumpStartTime;
                if (activeTime.TotalMinutes >= 180) 
                {
                    electricPump.Toggle();
                    electricPumpState = PumpState.Overheated;
                    overheatStartTime = simulationTime;
                    UpdateElectricPumpImage();
                }
            }

            // Cooldown after overheating
            if (electricPumpState == PumpState.Overheated)
            {
                var cooldownTime = simulationTime - overheatStartTime;
                if (cooldownTime.TotalMinutes >= 60) 
                {
                    electricPumpState = PumpState.Ready;
                    UpdateElectricPumpImage();
                }
            }

            //Calculate water level height for water tower
            waterLevelHeight = (int)(towerRect.Height * (tower.WaterLevel / (float)tower.MaxCapacity));
            UpdateStatus();
            this.Invalidate();
        }

        //Update status of status label
        private void UpdateStatus()
        {
            statusLabel.Text = $"Status: {tower.Status} | Water level: {tower.WaterLevel} liters";
        }
        
        //Update pump image
        private void UpdatePumpImage()
        {
            try
            {
                //Getting picture from path
                pumpPicture.Image = Image.FromFile(
                    Path.Combine("Images", $"pump_{(pump.IsActive ? "on" : "off")}.png")
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine( ex.Message );
            }
        }

        //Update ElectricPump image
        private void UpdateElectricPumpImage()
        {
            try
            {
                string imageName = "electric_pump_";
                if (electricPumpState == PumpState.Overheated) //Check if overheated
                    imageName += "overheated.png";
                else if (electricPump.IsActive)
                    imageName += "on.png";
                else
                    imageName += "off.png";
                //Getting image from path
                electricPumpPicture.Image = Image.FromFile(
                    Path.Combine("Images", imageName)
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
            }
        }

        //Allows turn on/off pumps
        protected override void OnMouseClick(MouseEventArgs e)
        {
            // Check if the user clicked within the bounds of the pump image
            if (pumpPicture.Bounds.Contains(e.Location))
            {
                // Toggle the pump state (on/off)
                pump.Toggle();
                // Update the pump image based on the new state (on/off)
                UpdatePumpImage();
            }
            // Check if the user clicked within the bounds of the electric pump image
            else if (electricPumpPicture.Bounds.Contains(e.Location))
            {
                // If the electric pump is in the Ready status, allow toggling
                if (electricPumpState == PumpState.Ready)
                {
                    // Toggle the electric pump state (on/off)
                    electricPump.Toggle();
                    // If the electric pump is activated, store the current simulation time as the start time for calculating overheating time
                    if (electricPump.IsActive)
                        electricPumpStartTime = simulationTime;
                    // Update the electric pump image based on its new status
                    UpdateElectricPumpImage();
                }
            }
            // Call the base class's OnMouseClick method to ensure proper handling of the event
            base.OnMouseClick(e);
        }

    }

    //Water tower object
    public class WaterTower
    {
        public int WaterLevel { get; set; }
        public TowerStatus Status { get; set; }
        public int MaxCapacity { get; }

        public WaterTower(int capacity)
        {
            MaxCapacity = capacity;
            WaterLevel = 50;
            Status = TowerStatus.Descending;
        }

        //Methods for adjusting water level 
        public void AddWater(int amount)
        {
            WaterLevel = Math.Min(WaterLevel + amount, MaxCapacity);
            Status = WaterLevel >= MaxCapacity ? TowerStatus.Full : TowerStatus.Filling;
        }

        public void RemoveWater(int amount)
        {
            WaterLevel = Math.Max(WaterLevel - amount, 0);
            Status = WaterLevel == 0 ? TowerStatus.Empty : TowerStatus.Descending;
        }
    }

    //Pump object
    public class Pump
    {
        public bool IsActive { get; set; }
        public int FlowRate { get; }

        public Pump(int flowRate)
        {
            FlowRate = flowRate;
            IsActive = false;
        }

        public void Toggle()
        {
            IsActive = !IsActive;
        }
    }

    //Consumer object
    public class Consumer
    {
        public int ConsumptionRate { get; }

        public Consumer(int rate)
        {
            ConsumptionRate = rate;
        }
    }

    //Stating point of program
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles(); // By visual styles means buttons, texts, etc.
            Application.SetCompatibleTextRenderingDefault(false); //Turn off text rendering
            Application.Run(new WaterTowerForm()); //Starts application, opens WaterTowerForm
        }
    }
}