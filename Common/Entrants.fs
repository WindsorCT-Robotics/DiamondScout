namespace ParagonRobotics.DiamondScout.Common.Entrants

open ParagonRobotics.DiamondScout.Common.Robots
open ParagonRobotics.DiamondScout.Common.Teams

type Entrant =
    { Team: TeamId
      Robot: RobotId
      PitScoutResults: Map<ParameterDefinition }
