using System.Collections.Generic;
using UnityEngine;

namespace CocaCopa {
    public class Utilities {

        /// <summary>
        /// Start a timer with the specified values
        /// </summary>
        /// <param name="waitTimer">Current timer value.</param>
        /// <param name="waitTime">Maximum timer value.</param>
        /// <param name="autoReset">True,  if you prefer the timer to reset automatically.</param>
        /// <returns>True, when the timer value is 0</returns>
        public static bool TickTimer(ref float waitTimer, float waitTime, bool autoReset = true) {
            if (waitTimer == 0) {
                if (autoReset)
                waitTimer = waitTime;
                return true;
            }
            else {
                waitTimer -= Time.deltaTime;
                waitTimer = Mathf.Clamp(waitTimer, 0, waitTime);
            }
            return false;
        }

        /// <summary>
        /// Interpolate an animation curve based on distance and speed.
        /// </summary>
        /// <param name="curve">The animation curve to interpolate.</param>
        /// <param name="animationPoints">The progression along the curve.</param>
        /// <param name="speed">The speed of interpolation in meters per second (m/s).</param>
        /// <param name="distance">The total distance covered by the animation.</param>
        /// <param name="increment">Set to 'false' to interpolate in reverse.</param>
        /// <returns>The value on the curve corresponding to the progression.
        /// </returns>
        public static float EvaluateAnimationCurve(AnimationCurve curve, ref float animationPoints, float speed, float distance, bool increment = true) {
            if (increment)
                animationPoints += (speed / distance) * Time.deltaTime;
            else
                animationPoints -= (speed / distance) * Time.deltaTime;
            animationPoints = float.IsNaN(animationPoints) ? 0 : Mathf.Clamp01(animationPoints);
            return curve.Evaluate(animationPoints);
        }

        /// <summary>
        /// Interpolate an animation curve based on speed.
        /// </summary>
        /// <param name="curve">The animation curve to interpolate.</param>
        /// <param name="animationPoints">The progression along the curve.</param>
        /// <param name="speed">The speed of interpolation in meters per second (m/s).</param>
        /// <param name="increment">Set to 'false' to interpolate in reverse.</param>
        /// <returns>The value on the curve corresponding to the progression.
        /// </returns>
        public static float EvaluateAnimationCurve(AnimationCurve curve, ref float animationPoints, float speed, bool increment = true) {
            if (increment)
                animationPoints += speed * Time.deltaTime;
            else
                animationPoints -= speed * Time.deltaTime;
            animationPoints = float.IsNaN(animationPoints) ? 0 : Mathf.Clamp01(animationPoints);
            return curve.Evaluate(animationPoints);
        }

        /// <summary>
        /// Swaps the values between 2 given Vectors
        /// </summary>
        /// <param name="vector_1">Value 1.</param>
        /// <param name="vector_2">Value 2.</param>
        public static void SwapVectorValues(ref Vector3 vector_1, ref Vector3 vector_2) {
            Vector3 temp = vector_1;
            vector_1 = vector_2;
            vector_2 = temp;
        }

        /// <summary>
        /// Swaps the values between the 2 given floats
        /// </summary>
        /// <param name="float_1">Value 1.</param>
        /// <param name="float_2">Value 2.</param>
        public static void SwapFloatValues(ref float float_1, ref float float_2) {
            float temp = float_1;
            float_1 = float_2;
            float_2 = temp;
        }

        /// <summary>
        /// Generate a random Vector3 inside the given circle
        /// </summary>
        /// <param name="center">Center of the circle.</param>
        /// <param name="radius">Radius of the circle.</param>
        /// <param name="forwardDirection">Orientation of the circle.</param>
        /// <returns>The generated Vector3</returns>
        public static Vector3 RandomVectorPointInCircle(Vector3 center, float radius, Vector3 forwardDirection) {
            var vector2 = Random.insideUnitCircle * radius;
            float signedAngle = Vector3.SignedAngle(forwardDirection, vector2, Vector3.up);
            if (signedAngle > 90 || signedAngle < -90) {
                vector2 = -vector2;
            }
            return new Vector3(vector2.x, 0, vector2.y) + center;
        }

        /// <summary>
        /// Checks if the given Vector3 is inside the given box.
        /// </summary>
        /// <param name="transform">Transfom of the object calling the function.</param>
        /// <param name="point">Position to check.</param>
        /// <param name="limitsX">Box size X.</param>
        /// <param name="limitsZ">Box size Z.</param>
        /// <returns>True, if the given point is out of the specified limits.</returns>
        public static bool OutOfBounds(Transform transform, Vector3 point, float limitsX, float limitsZ) {
            Vector3 pointWorldToLocal = transform.InverseTransformPoint(point);
            bool outOfBoundsUp    = pointWorldToLocal.z - limitsZ / 2 > 0;
            bool outOfBoundsDown  = pointWorldToLocal.z + limitsZ / 2 < 0;
            bool outOfBoundsRight = pointWorldToLocal.x - limitsX / 2 > 0;
            bool outOfBoundsLeft  = pointWorldToLocal.x + limitsX / 2 < 0;
            return outOfBoundsUp || outOfBoundsDown || outOfBoundsRight || outOfBoundsLeft;
        }

        /// <summary>
        /// Checks if the given position is close to any of the positions in a list.
        /// </summary>
        /// <param name="position">Position to check.</param>
        /// <param name="positions">List of positions.</param>
        /// <param name="distance">Distance.</param>
        /// <returns>True, if the distance is less than the specified 'distance'.</returns>
        public static bool IsPositionCloseToAnyPosition(Vector3 position, List<Vector3> positions, float distance) {
            bool tooClose = false;
            for (int i = 0; i < positions.Count; i++) {
                if (Vector3.Distance(position, positions[i]) <= distance) {
                    tooClose = true;
                    break;
                }
            }
            return tooClose;
        }

        /// <summary>
        /// Checks if the given position is close to any of the positions in a list
        /// </summary>
        /// <param name="position">Position to check</param>
        /// <param name="positions">List of positions</param>
        /// <param name="distances">List of distances</param>
        /// <returns>True, if the distance is less than the specified 'distances'</returns>
        public static bool IsPositionCloseToAnyPosition(Vector3 position, List<Vector3> positions, List<float> distances) {
            bool tooClose = false;
            for (int i = 0; i < positions.Count; i++) {
                if (Vector3.Distance(position, positions[i]) <= distances[i]) {
                    tooClose = true;
                    break;
                }
            }
            return tooClose;
        }

        /// <summary>
        /// Plays a random sound picked from the given sound array.
        /// </summary>
        /// <param name="audioSource">Target audio source</param>
        /// <param name="soundsArray">List to choose sound from</param>
        /// <param name="setAudioClip">Set to 'true' if you want to set the selected clip as the audioSource's clip</param>
        public static void PlayRandomSound(AudioSource audioSource, AudioClip[] soundsArray, bool setAudioClip = false) {
            if (soundsArray.Length > 0) {
                int randomIndex = Random.Range(0, soundsArray.Length);
                AudioClip audioClip = soundsArray[randomIndex];
                if (setAudioClip) {
                    audioSource.clip = audioClip;
                }
                audioSource.PlayOneShot(audioClip);
            }
        }
    }
}