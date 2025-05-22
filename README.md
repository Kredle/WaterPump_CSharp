
# Water Tower Simulation

A C# Windows Forms application that simulates a water tower system with water flow logic, pump controls, consumer demand, and visual UI.

---

## Features

- Simulates a water tower with real-time water level changes.
- Supports two pumps:  
  - **Main Pump** (manual/auto)  
  - **Electric Pump** (auto with overheating logic)
- Visual UI with:
  - Tower water level rendering
  - Houses consuming water
  - Status messages when water is low
  - Time-based simulation clock (10 mins by default)
  - Funny text "Where is my water", which shows when our consumers stop getting water (when our tower is in filling state)
- Overheat mechanic for electric pump (runs max 3h, then cools down 1h).

---

## Required Images

Put the following images in an `Images/` folder located in the same directory as your executable:

- `pump_on.png`
- `pump_off.png`
- `electric_pump_on.png`
- `electric_pump_off.png`
- `electric_pump_overheated.png`
- `house.png`

> Make sure filenames match exactly and that the folder is named `Images`.

---

## How to Run

1. Clone or download the repository.
2. Open in **Visual Studio**.
3. Ensure images are in the `Images/` folder.
4. Ensure you have **.NET 7.0 SDK** installed.
5. Build and run the application.

---

## Simulation Logic

- The water tower has a max capacity (default: 1000 liters).
- Consumers (default: 2) use a combined total of 45 liters/hour.
- Pumps add water at a rate of 80 liters/hour.
- Every simulation tick represents **10 minutes** of real time.
- The electric pump will:
  - Overheat if it runs for more than 3 simulated hours.
  - Cool down after 1 simulated hour and can be used again.

---

## Technologies Used

- C# (.NET Framework)
- Windows Forms
- System.Drawing
- Basic OOP principles

---

## Customization

You can customize:

- Number of consumers and their rates (`InitializeSimulation` method).
- Number of pumps.
- Water tower capacity.
- Pump flow rates.
- Overheating durations (via code constants).

---

## Credits

Developed for the C# course at Ivan Franko National University of Lviv.
