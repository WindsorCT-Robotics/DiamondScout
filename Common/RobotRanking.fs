namespace ParagonRobotics.DiamondScout.Common

type RobotRank =
    private
        { Robot: RobotId
          RankingPoints: RankingPoints
          MatchesPlayed: uint
          MatchesWon: uint }

type RobotRankings =
    private
        { RankByEvent: Map<EventId, RobotRank>
          RankByDistrict: Map<FrcDistrictCode, RobotRank> }