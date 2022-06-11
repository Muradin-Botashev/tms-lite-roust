import React from 'react';

import {
    ACTIVE_TYPE,
    BOOLEAN_TYPE,
    CUSTOM_SELECT_TYPE,
    CUSTOM_STATE_TYPE,
    ENUM_TYPE,
    LABELS_TYPE,
    LINK_TYPE,
    LOCAL_DATE_TIME,
    NUMBER_TYPE,
    VALIDATED_NUMBER_TYPE,
    SELECT_TYPE,
    STATE_TYPE,
    MULTI_ENUM_TYPE,
    MULTI_SELECT_TYPE,
    TRANSPORT_COMPANIES_TYPE,
} from '../../constants/columnTypes';
import { numbersFormat } from '../../utils/numbersFormat';
import { Checkbox, Label } from 'semantic-ui-react';
import StateValue from './StateValue';
import SelectValue from './SelectValue';
import TextCropping from './TextCropping';
import { dateToUTC } from '../../utils/dateTimeFormater';
import ToggleCheckbox from './ToogleCheckbox';
import TransportCompanies from './TransportCompanies';

const CellValue = ({
    type,
    value = '',
    valueText,
    valueTooltip,
    name,
    id,
    toggleIsActive,
    source,
    indexRow,
    indexColumn,
    modalCard,
    showRawValue,
    width,
    t,
    isDisabled,
    cardLink,
    gridName,
    rowId,
    goToCard,
    decimals,
    alternativeCosts,
    ...extProps
}) => {
    if (type === TRANSPORT_COMPANIES_TYPE) {
        return (
            <TransportCompanies
                id={rowId}
                value={value}
                valueText={valueText}
                width={width}
                source={source}
                indexColumn={indexColumn}
                indexRow={indexRow}
                alternativeCosts={alternativeCosts}
                runId={extProps.runId}
                loadData={extProps.loadData}
            />
        );
    }

    if (type === SELECT_TYPE || type === CUSTOM_SELECT_TYPE) {
        return (
            <SelectValue
                width={width}
                value={value}
                valueText={valueText}
                source={source}
                indexRow={indexRow}
                indexColumn={indexColumn}
                showRawValue={showRawValue}
            />
        );
    }

    if (type === STATE_TYPE || type === CUSTOM_STATE_TYPE) {
        return (
            <StateValue
                width={width}
                value={value}
                source={source}
                indexRow={indexRow}
                indexColumn={indexColumn}
            />
        );
    }

    if (type === LABELS_TYPE) {
        return (
            <>
                {!value
                    ? t('All')
                    : value.map((n, i) => (
                          <Label key={n.name} className="label-margin">
                              {t(n.name)}
                          </Label>
                      ))}
            </>
        );
    }

    if (type === ENUM_TYPE) {
        return (
            <TextCropping width={width} indexColumn={indexColumn}>
                {value ? valueText : ''}
            </TextCropping>
        );
    }

    if (type === ACTIVE_TYPE) {
        return (
            <ToggleCheckbox
                id={id}
                value={value}
                disabled={isDisabled}
                toggleIsActive={toggleIsActive}
            />
        );
    }

    if (type === BOOLEAN_TYPE) {
        return <>{value === true ? t('Yes') : value === false ? t('No') : ''}</>;
    }

    if (type === MULTI_ENUM_TYPE || type === MULTI_SELECT_TYPE) {
        return value
            ? value.reduce((str, item) => `${str}${str.length ? ', ' : ''}${item.name}`, '')
            : '';
    }

    if (type === VALIDATED_NUMBER_TYPE) {
        let cellValue;
        if (valueText !== undefined && valueText !== null && valueText) {
            cellValue = valueText;
        } else if (decimals !== undefined && decimals !== null) {
            cellValue = new Intl.NumberFormat().format(
                numbersFormat(parseFloat(value).toFixed(decimals)),
            ); // new Intl.NumberFormat().format() чтоб разделитель дробной части брался из настроек системы)
        } else {
            cellValue = new Intl.NumberFormat().format(numbersFormat(parseFloat(value)));
        }

        if (valueTooltip !== undefined && valueTooltip !== null && valueTooltip.length > 0) {
            return <span title={valueTooltip}>{cellValue}</span>;
        }
        return <>{cellValue}</>;
    }

    if (value === undefined || value === null) return '';

    if (type === NUMBER_TYPE) {
        if (decimals !== undefined && decimals !== null) {
            return (
                <>
                    {new Intl.NumberFormat().format(
                        numbersFormat(parseFloat(value).toFixed(decimals)),
                    )}
                </>
            ); // new Intl.NumberFormat().format() чтоб разделитель дробной части брался из настроек системы)
        }
        return <>{new Intl.NumberFormat().format(numbersFormat(parseFloat(value)))}</>;
    }

    if (type === LINK_TYPE) {
        const handleGoToCard = () => {
            goToCard(true, rowId, source);
        };

        return (
            <>
                {goToCard ? (
                    <div className="link-cell" onClick={handleGoToCard}>
                        <TextCropping width={width} indexColumn={indexColumn}>
                            {value}
                        </TextCropping>
                    </div>
                ) : (
                    <TextCropping width={width} indexColumn={indexColumn}>
                        {value}
                    </TextCropping>
                )}
            </>
        );
    }

    if (type === LOCAL_DATE_TIME) {
        return (
            <TextCropping width={width} indexColumn={indexColumn}>
                {dateToUTC(value, 'DD.MM.YYYY HH:mm')}
            </TextCropping>
        );
    }

    return (
        <TextCropping width={width} indexColumn={indexColumn}>
            {value}
        </TextCropping>
    );
};

export default React.memo(CellValue);
