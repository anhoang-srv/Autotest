Feature: Narrator checks
  As a user I want the narrator to read the focused control text on the Get Started screen

  Scenario: Check Narrator on Get Started screen
    Given "Samsung Cloud" app is launched
    And the "App Name Screen" is clicked
    When the "Get Started" button is focused
    Then the Narrator must be reading "Get Started"