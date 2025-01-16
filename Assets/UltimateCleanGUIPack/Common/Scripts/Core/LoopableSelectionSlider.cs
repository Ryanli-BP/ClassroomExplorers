// Copyright (C) 2019 gamevanilla - All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at https://unity.com/legal/as-terms.

namespace UltimateClean
{
    /// <summary>
    /// A loopable selection slider.
    /// </summary>
    public class LoopableSelectionSlider : SelectionSlider
    {
        public override void OnPreviousButtonPressed()
        {
            --currentIndex;
            if (currentIndex < 0)
            {
                currentIndex = Options.Count - 1;
            }

            SetCurrentOptionLabel();
        }

        public override void OnNextButtonPressed()
        {
            ++currentIndex;
            if (currentIndex > Options.Count - 1)
            {
                currentIndex = 0;
            }

            SetCurrentOptionLabel();
        }
    }
}