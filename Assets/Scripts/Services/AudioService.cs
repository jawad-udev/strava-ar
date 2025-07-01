using UnityEngine;
using System.Collections;

public class AudioService : MonoBehaviour
{
	public bool isSoundEnable = true;
	public bool isMusicEnable = true;

	#region Gameplay Spesific
	public AudioClip shapePlaceSound;
	public AudioClip shapeMoveSound;
	public AudioClip explosionSound;
	public AudioClip blockSpawnSound;
	public AudioClip timeCountDownSound;
    public AudioClip shapeRotateSound;
    #endregion

    public AudioClip gameMusic;
	public AudioClip uiClick;
	public AudioClip winSound;
	public AudioClip loseSound;
	public AudioClip popUpOpen;
	public AudioClip popUpClose;

    #region Audio Sources Fields
    public AudioSource musicSource;
	public AudioSource soundSource;
	#endregion


	#region Sound FX Methods
	public void PlayLoseSound()
	{
		if (!isSoundEnable)
			return;

		StopGameMusic();
		soundSource.PlayOneShot(loseSound);
	}

	public void PlayUIClick()
	{
		if (!isSoundEnable)
			return;

		soundSource.PlayOneShot(uiClick);
	}

	public void PlayWinSound()
	{
		if (!isSoundEnable)
			return;

		StopGameMusic();
		soundSource.PlayOneShot(winSound);
	}

	public void PlaySplashScreenSound()
	{
		if (!isSoundEnable)
			return;

	}

	public void PlayPopUpOpenSound()
	{
		if (!isSoundEnable)
			return;

		soundSource.PlayOneShot(popUpOpen);
	}

	public void PlayPopUpCloseSound()
	{
		if (!isSoundEnable)
			return;

	}

	public void PlayShapePlaceSound()
	{
		if (!isSoundEnable)
			return;

		soundSource.PlayOneShot(shapePlaceSound);
	}

	public void PlayShapeMoveSound()
	{
		if (!isSoundEnable)
			return;

		soundSource.PlayOneShot(shapeMoveSound);
	}

	public void PlayExplosionSound()
	{
		if (!isSoundEnable)
			return;

		soundSource.PlayOneShot(explosionSound);
	}

	public void PlayBlockSpawnSound()
	{
		if (!isSoundEnable)
			return;

		soundSource.PlayOneShot(blockSpawnSound);
	}

	public void PlayTimeCountDownSound()
	{
		if (!isSoundEnable)
			return;

		soundSource.PlayOneShot(timeCountDownSound);
	}

    public void PlayShapeRotateSound()
    {
        if (!isSoundEnable)
            return;

        soundSource.PlayOneShot(shapeRotateSound);
    }

    public void SetSoundFxVolume(float value)
	{
		float temp = value + soundSource.volume;
		if (temp < 0 || temp > 1)
			return;
		else
			soundSource.volume += value;
	}
	#endregion

	#region Music Methods

	public void EnableGameMusic(bool enable)
    {
		isMusicEnable = enable;

		if (isMusicEnable)
			PlayGameMusic();
		else
			StopGameMusic();

	}

	public void PlayGameMusic()
	{
		musicSource.clip = gameMusic;
		musicSource.Play();
	}

    public void RestartGameMusic()
    {
        StopGameMusic();
        musicSource.clip = gameMusic;
        musicSource.Play();
    }

    public void StopGameMusic()
	{
		musicSource.Stop();
	}

	public void SetSoundMusicVolume(float value)
	{
		float temp = value + musicSource.volume;
		if (temp < 0 || temp > 1)
			return;
		else
			musicSource.volume += value;
	}
	#endregion

}