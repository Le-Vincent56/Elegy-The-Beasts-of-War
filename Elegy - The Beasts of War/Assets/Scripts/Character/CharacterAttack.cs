using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elegy.Grid;


namespace Elegy.Characters
{
    public class CharacterAttack : MonoBehaviour
    {
        [SerializeField] GridObject selectedCharacter;

        public void CalculateAttackArea(Component sender, object data)
        {
            if(data is Grid.Grid)
            {
                // Cast data
                Grid.Grid targetGrid = (Grid.Grid)data;

                // Establish attack range
                Character character = selectedCharacter.GetComponent<Character>();
                int attackRange = character.attackRange;

                // Create a positions list to hold attack positions
                List<Vector2Int> positions = new List<Vector2Int>();

                // Loop through the possible ranges
                for (int x = -attackRange; x <= attackRange; x++)
                {
                    for (int y = -attackRange; y <= attackRange; y++)
                    {
                        // If the total distance if above the attack range, then skip
                        if(Mathf.Abs(x) + Mathf.Abs(y) > attackRange)
                        {
                            continue;
                        }

                        // Check if the position is on the grid
                        if(targetGrid.CheckBounds(selectedCharacter.gridPos.x + x, selectedCharacter.gridPos.y + y))
                        {
                            // If so, add it to the positions list
                            positions.Add(new Vector2Int(selectedCharacter.gridPos.x + x, selectedCharacter.gridPos.y + y));
                        }
                    }
                }
            }
        }
    }
}

