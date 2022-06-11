import {
    DATE_TIME_TYPE,
    NUMBER_TYPE,
    SELECT_TYPE,
    STATE_TYPE,
    TEXT_TYPE,
} from '../constants/columnTypes';

export default {
    userName: 'Иванов Иван Иванович',
    role: 'admin',
    grids: [
        {
            name: 'routes',
            columns: [
                { key: 'status', type: STATE_TYPE, text: 'Статус', width: 200 },
                { key: 'created_at', type: DATE_TIME_TYPE, text: 'Дата создания', width: 150 },
                { key: 'route_code_ltl', type: TEXT_TYPE, text: 'Номер рейса', width: 100 },
                { key: 'car_number', type: SELECT_TYPE, text: 'Номер ТС', width: 100 },
                { key: 'trailer_number', type: SELECT_TYPE, text: 'Номер прицепа', width: 100 },
                { key: 'route_code_otd', type: TEXT_TYPE, text: 'Номер рейса OTD', width: 100 },
                { key: 'vehicle_tonnage', type: NUMBER_TYPE, text: 'Грузоподъемность', width: 70 },
                {
                    key: 'vehicle_pallet_capacity',
                    type: NUMBER_TYPE,
                    text: 'Паллетовместимость',
                    width: 70,
                },
                { key: 'density', type: TEXT_TYPE, text: 'Емкость', width: 100 },
            ],
        },
        {
            name: 'orders',
            columns: [],
        },
    ],
    dictionaries: [
        {
            name: 'drivers',
            columns: [{ key: 'key1' }, { key: 'key2' }],
        },
        {
            name: 'car',
            columns: [],
        },
    ],
};
