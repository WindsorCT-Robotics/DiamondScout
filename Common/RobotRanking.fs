namespace ParagonRobotics.DiamondScout.Common.Functional

open ParagonRobotics.DiamondScout.Common

type RobotRank =
    private
        { Robot: RobotId
          RankingPoints: RankingPoints
          MatchesPlayed: uint
          MatchesWon: uint }

type RobotRankings =
    private
        { RankByEvent: Map<FrcEventId, RobotRank>
          RankByDistrict: Map<FrcDistrictCode, RobotRank> }
