Feature: Narrator checks
  As a user I want the narrator to read the focused control text on the Get Started screen

  Scenario: Check Narrator on Samsung Cloud screen
    Given "Samsung Cloud" app is launched
    When the "Cancel" button is focused
    Then the Narrator must be reading "Cancel button"
    When the "Install" button is focused
    Then the Narrator must be reading "Install button"  
