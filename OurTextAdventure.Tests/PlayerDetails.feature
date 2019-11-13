Feature: Describe Player

    Scenario: Describe the player
        Given a player Bob that can be described as an awesome banjo player
        When I would describe the player
        Then I would say Bob is an awesome banjo player

    Scenario Outline: Describe players
        Given a player <Name> that can be described as <Description>
        When I would describe the player
        Then I would say <Name> is <Description>

        Examples:
            | Name        | Description                                        |
            | Bob         | an awesome banjo player                            |
            | Action Hank | a man wielding the most rugged beard known to man. |