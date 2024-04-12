using UnityEngine;

namespace Generator
{
    public class BinarySpaceTrees
    {
        private LevelGenerator levelGenerator;

        public BinarySpaceTrees(LevelGenerator pLevelGenerator)
        {
            levelGenerator = pLevelGenerator;
        }

        public void Partition(Island island)
        {
            if (island.getWidth() <= levelGenerator.m_maxIslandWidth && island.getHeight() <= levelGenerator.m_maxIslandHeight) {
                island.setIsLeaf();
                levelGenerator.leafIslands.Add(island);
                return;
            }

            if (island.getWidth() > levelGenerator.m_maxIslandHeight && island.getHeight() > levelGenerator.m_maxIslandHeight) {
                if (Random.Range(0, 100) < 50) {
                    // X - horizontal split
                    SplitHorizontally(island);
                } else {
                    // Y - vertical split
                    SplitVertically(island);
                }
            } else if (island.getWidth() > levelGenerator.m_maxIslandHeight) {
                // X - horizontal split
                SplitHorizontally(island);
            } else {
                // Y - vertical split
                SplitVertically(island);
            }
            Partition(island.getLeftIsland());
            Partition(island.getRightIsland());
        }

        private void SplitHorizontally(Island island)
        {
            Vector2Int topLeft = island.getTopLeft(), downRight = island.getDownRight();
            int xMid = (topLeft.x + downRight.x) / 2;
            xMid = Random.Range(xMid - levelGenerator.m_splitWiggle, xMid + levelGenerator.m_splitWiggle);
            island.setLeftIsland(new Island(topLeft.x, topLeft.y, xMid, downRight.y, levelGenerator.m_islandPadding));
            island.setRightIsland(new Island(xMid, topLeft.y, downRight.x, downRight.y, levelGenerator.m_islandPadding));
        }

        private void SplitVertically(Island island)
        {
            Vector2Int topLeft = island.getTopLeft(), downRight = island.getDownRight();
            int yMid = (topLeft.y + downRight.y) / 2;
            yMid = Random.Range(yMid - levelGenerator.m_splitWiggle, yMid + levelGenerator.m_splitWiggle);
            island.setLeftIsland(new Island(topLeft.x, topLeft.y, downRight.x, yMid, levelGenerator.m_islandPadding));
            island.setRightIsland(new Island(topLeft.x, yMid, downRight.x, downRight.y, levelGenerator.m_islandPadding));
        }
    }
}
