using System;
using RWave.Enum;
using UnityEngine;

namespace RWave.Data.Setting
{
    /// <summary>
    /// RWaveのCrossFadeの設定を定義するClass
    /// </summary>
    [Serializable]
    public class RWaveCrossFadeSetting
    {
        /// <summary>
        /// CrossFadeが有効化されているかどうか
        /// </summary>
        public bool isCrossFadeActive => _crossFadeActiveFlag == eRWaveActiveFlag.Enable;
        
        /// <summary>
        /// CrossFadeの有効化
        /// </summary>
        public eRWaveActiveFlag crossFadeActiveFlag => _crossFadeActiveFlag;
        
        /// <summary>
        /// Attack(ms)
        /// </summary>
        public int attackTime => _attackTime;
        
        /// <summary>
        /// Release(ms)
        /// </summary>
        public int releaseTime => _releaseTime;
        
        /// <summary>
        /// Attack秒数(float)
        /// </summary>
        public float attackTimeInSeconds => _attackTime / 1000f;

        /// <summary>
        /// Release秒数(float)
        /// </summary>
        public float releaseTimeInSeconds => _releaseTime / 1000f;
        
        [SerializeField,Header("CrossFadeの有効化[有効化されている場合、同時発音数は1で固定されます]")]
        private eRWaveActiveFlag _crossFadeActiveFlag = eRWaveActiveFlag.Disable;
        [SerializeField,Header("Attack(ms)")]
        private int _attackTime = 300;
        [SerializeField,Header("Release(ms)")]
        private int _releaseTime = 300;
    }
}