public interface IMapUI
{
	void Enter();

	void Leave();

	void ActivateCampButton();

	void DeactivateCampButton();

	void ComeBackFromDailyLogin();
	
	void OnNewsButtonClicked();

	bool IsActive();
}
