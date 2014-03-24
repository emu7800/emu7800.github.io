using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;

using EMU7800.Core;
using EMU7800.WP.Model;

namespace EMU7800.WP.ViewModel
{
    public class GameProgramSelectViewModel
    {
        #region Fields

        readonly IEnumerable<GameProgramSelectItemViewModel> _viewModelRepository;

        #endregion

        public IEnumerable<GameProgramSelectItemViewModel> Games2600       { get; private set; }
        public IEnumerable<GameProgramSelectItemViewModel> Games7800       { get; private set; }
        public IEnumerable<GameProgramSelectItemViewModel> GamesAtari      { get; private set; }
        public IEnumerable<GameProgramSelectItemViewModel> GamesActivision { get; private set; }
        public IEnumerable<GameProgramSelectItemViewModel> GamesImagic     { get; private set; }
        public IEnumerable<GameProgramSelectItemViewModel> GamesOther      { get; private set; }

        public GameProgramSelectItemViewModel GetGameProgramSelectItemViewModel(GameProgramId id)
        {
            var query = from i in _viewModelRepository
                        where i.Id == id
                        select i;
            return query.FirstOrDefault();
        }

        public void SetPausedState(GameProgramId id, bool isPaused)
        {
            var gp = _viewModelRepository.FirstOrDefault(r => r.Id == id);
            if (gp != null)
                gp.IsPaused = isPaused;
        }

        #region Constructors

        public GameProgramSelectViewModel(GameProgramInfoRepository repository)
        {
            if (repository == null)
                throw new ArgumentNullException("repository");

            using (var userStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                var us = userStore;
                var query = from gi in repository.GetAllGamePrograms()
                            select new GameProgramSelectItemViewModel(gi, us.FileExists(StateUtils.ToSerializationFileName(gi.Id)));
                _viewModelRepository = query.ToList();
            }

            var q2600       = from gi in _viewModelRepository
                              where (gi.MachineType == MachineType.A2600NTSC || gi.MachineType == MachineType.A2600PAL)
                                 && !gi.Manufacturer.Equals(GameProgramInfoRepository.ManufacturerActivision)
                              orderby gi.Title ascending
                              select gi;
            Games2600       = q2600.AsEnumerable();

            var q7800       = from gi in _viewModelRepository
                              where (gi.MachineType == MachineType.A7800NTSC || gi.MachineType == MachineType.A7800PAL)
                                 && !gi.Manufacturer.Equals(GameProgramInfoRepository.ManufacturerActivision)
                              orderby gi.Title ascending
                              select gi;
            Games7800       = q7800.AsEnumerable();

            var qAtari      = from gi in _viewModelRepository
                              where gi.Manufacturer.Equals(GameProgramInfoRepository.ManufacturerAtari)
                              orderby gi.Title ascending
                              select gi;
            GamesAtari      = qAtari.AsEnumerable();

            var qImagic     = from gi in _viewModelRepository
                              where gi.Manufacturer.Equals(GameProgramInfoRepository.ManufacturerImagic)
                              orderby gi.Title ascending
                              select gi;
            GamesImagic     = qImagic.AsEnumerable();

            var qActivision = from gi in _viewModelRepository
                              where gi.Manufacturer.Equals(GameProgramInfoRepository.ManufacturerActivision)
                              orderby gi.Title ascending
                              select gi;
            GamesActivision = qActivision.AsEnumerable();

            var qOther      = from gi in _viewModelRepository
                              where !gi.Manufacturer.Equals(GameProgramInfoRepository.ManufacturerAtari)
                                 && !gi.Manufacturer.Equals(GameProgramInfoRepository.ManufacturerImagic)
                                 && !gi.Manufacturer.Equals(GameProgramInfoRepository.ManufacturerActivision)
                              orderby gi.Title ascending
                              select gi;
            GamesOther      = qOther.AsEnumerable();
        }

        #endregion
    }
}
