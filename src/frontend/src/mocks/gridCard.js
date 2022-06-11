import { TEXT_TYPE } from '../constants/columnTypes';

export const TABLE_TAB_TYPE = 'table';
export const ROW_TAB_TYPE = 'row';

export default {
    title: 'order {12}',
    tabs: [
        {
            name: 'Информация',
            views: [
                {
                    type: ROW_TAB_TYPE,
                    fields: [
                        {
                            name: '',
                            type: TEXT_TYPE,
                            isEdit: true,
                        },
                    ],
                },
                {
                    type: ROW_TAB_TYPE,
                    fields: [
                        {
                            name: '',
                            type: TEXT_TYPE,
                        },
                    ],
                },
                {
                    type: ROW_TAB_TYPE,
                    fields: [
                        {
                            name: '',
                            type: '',
                        },
                    ],
                },
            ],
        },
        {
            name: 'Маршрут',
            views: [
                {
                    type: TABLE_TAB_TYPE,
                    fields: [
                        {
                            name: '',
                            type: '',
                        },
                    ],
                },
            ],
        },
        {
            name: 'Позиции',
        },
        {
            name: 'Возвраты',
        },
        {
            name: 'Документы',
        },
        {
            name: 'История',
        },
    ],
};
