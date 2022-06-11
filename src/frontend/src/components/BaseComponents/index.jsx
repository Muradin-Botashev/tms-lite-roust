import React, { useEffect } from 'react';
import { useDispatch } from 'react-redux';
import {
    BIG_TEXT_TYPE,
    BOOLEAN_TYPE,
    CHECKBOX_TYPE,
    DATE_TIME_TYPE,
    DATE_TYPE,
    ENUM_TYPE,
    GROUP_TYPE,
    LOCAL_DATE_TIME,
    NUMBER_TYPE,
    VALIDATED_NUMBER_TYPE,
    SELECT_TYPE,
    SOLD_TO_TYPE,
    STATE_TYPE,
    TEXT_TYPE,
    TIME_TYPE,
    PASSWORD_TYPE, INTEGER_TYPE, RETURN_COST_TYPE, MULTI_SELECT_TYPE, MULTI_ENUM_TYPE, CONSTRUCTOR_TYPE,
} from '../../constants/columnTypes';
import Text from './Text';
import TextArea from './TextArea';
import State from './State';
import Date from './Date';
import Select from './Select';
import Bool from './Bool';
import DateTime from './DateTime';
import { SETTINGS_TYPE_HIDE, SETTINGS_TYPE_SHOW } from '../../constants/formTypes';
import CheckBox from './Checkbox';
import { clearError } from '../../ducks/gridCard';
import SoldToField from './SoldToField';
import PasswordField from './Password';
import Number from "./Number";
import ReturnCosts from "./ReturnCosts";
import MultiSelect from "./MultiSelect";
import Constructor from "./Constructor";

const getTypeFacet = {
    [TEXT_TYPE]: <Text />,
    [STATE_TYPE]: <State />,
    [DATE_TYPE]: <Date />,
    [DATE_TIME_TYPE]: <DateTime />,
    [LOCAL_DATE_TIME]: <DateTime />,
    [TIME_TYPE]: <Text />,
    [SELECT_TYPE]: <Select />,
    [NUMBER_TYPE]: <Number/>,
    [VALIDATED_NUMBER_TYPE]: <Number/>,
    [BOOLEAN_TYPE]: <Bool />,
    [ENUM_TYPE]: <Select isTranslate />,
    [BIG_TEXT_TYPE]: <TextArea />,
    [CHECKBOX_TYPE]: <CheckBox />,
    [SOLD_TO_TYPE]: <SoldToField />,
    [RETURN_COST_TYPE]: <ReturnCosts/>,
    [PASSWORD_TYPE]: <PasswordField />,
    [INTEGER_TYPE]: <Number isInteger />,
    [MULTI_SELECT_TYPE]: <MultiSelect/>,
    [MULTI_ENUM_TYPE]: <MultiSelect isTranslate />,
    [CONSTRUCTOR_TYPE]: <Constructor />,
};

const FormField = props => {
    const dispatch = useDispatch();

    let params = {
        ...props,
        type: props.typeValue,
        key: props.name,
    };

    if (!props.text && props.displayNameKey) {
        params = {
            ...params,
            text: props.displayNameKey,
        };
    }

    if (props.type === TIME_TYPE) {
        params = {
            ...params,
            type: 'time',
        };
    }

    if ((props.settings && props.settings === SETTINGS_TYPE_SHOW) || props.isReadOnly) {
        params = {
            ...params,
            isDisabled: true,
        };
    }

    if (props.settings && props.settings === SETTINGS_TYPE_HIDE) {
        params = {
            ...params,
            isDisabled: true,
            value: null,
        };
    }

    useEffect(
        () => {
            if (props.error) {
                dispatch(clearError && clearError(props.name));
            }
        },
        [props.value],
    );

    /* switch (props.type || (props.column && props.column.type)) {
         case TEXT_TYPE:
             return <Text {...params} />;
         case STATE_TYPE:
             return <State {...params} />;
         case DATE_TYPE:
             return <Date {...params} />;
         case DATE_TIME_TYPE:
             return <DateTime {...params} />;
         case TIME_TYPE:
             return <Text type="time" {...params} />;
         case SELECT_TYPE:
             return <Select {...params} />;
         case NUMBER_TYPE:
             return <Text {...params} />;
         case BOOLEAN_TYPE:
             return <Bool {...params} />;
         case ENUM_TYPE:
             return <Select isTranslate {...params} />;
         case BIG_TEXT_TYPE:
             return <TextArea {...params} />;
         case CHECKBOX_TYPE:
             return <CheckBox {...params} />;
         default:
             return <Text {...params} />
     }*/

    return React.cloneElement(getTypeFacet[props.type] || <Text />, { ...params });
};

export default React.memo(FormField);
