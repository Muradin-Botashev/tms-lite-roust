import React, { useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useDispatch, useSelector } from 'react-redux';

import { Icon } from 'semantic-ui-react';
import { getLookupRequest, valuesListSelector } from '../../ducks/lookup';
import TextCropping from './TextCropping';

const StateValue = ({ value, source, indexRow, indexColumn, width }) => {
    const { t } = useTranslation();
    const dispatch = useDispatch();

    let stateColors = useSelector(state => valuesListSelector(state, source)) || [];

    useEffect(() => {
        if (!stateColors.length && indexRow === 0) {
            dispatch(
                getLookupRequest({
                    name: source,
                    isForm: true,
                    isState: true,
                }),
            );
        }
    }, []);

    const state = stateColors.find(x => x.value === value);
    const color = state ? state.color : 'grey';

    return (
        <div className="status-value">
            {value && (
                <TextCropping width={width} indexColumn={indexColumn}>
                    <Icon color={color && color.toLowerCase()} name="circle" />
                    {t(value)}
                </TextCropping>
            )}
        </div>
    );
};

export default StateValue;
