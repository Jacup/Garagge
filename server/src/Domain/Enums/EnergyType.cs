namespace Domain.Enums;

[Flags]
public enum EnergyType
{
    None = 0,

    Gasoline = 1 << 0,  // 1
    Diesel = 1 << 1,    // 2
    LPG = 1 << 2,       // 4
    CNG = 1 << 3,       // 8
    Ethanol = 1 << 4,   // 16
    Biofuel = 1 << 5,   // 32
    Hydrogen = 1 << 6,  // 64

    Electric = 1 << 7,  // 128

    AllFuels = Gasoline | Diesel | LPG | CNG | Ethanol | Biofuel | Hydrogen,
    AllCharging = Electric,
    All = AllFuels | AllCharging
}